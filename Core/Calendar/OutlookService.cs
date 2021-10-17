using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Extensions;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web.TokenCacheProviders;
using IntegratedCacheUtils.Stores;

namespace FamilyBoard.Core.Calendar
{
    public class OutlookService : ICalendarService
    {
        private readonly ILogger<OutlookService> _logger;

        private readonly IMsalAccountActivityStore _msalAccountActivityStore;
        private readonly IMsalTokenCacheProvider _msalTokenCacheProvider;
        private readonly IConfiguration _configuration;

        public OutlookService(ILogger<OutlookService> logger,
                            IConfiguration configuration,
                            IMsalAccountActivityStore msalAccountActivityStore,
                            IMsalTokenCacheProvider msalTokenCacheProvider)
        {
            _logger = logger;
            _configuration = configuration;
            _msalAccountActivityStore = msalAccountActivityStore;
            _msalTokenCacheProvider = msalTokenCacheProvider;
        }

        public bool IsHolidays => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public async Task<List<CalendarEntry>> GetEvents(DateTime startDate, DateTime endDate, bool isPrimary = false, bool isSecondary = false)
        {
            string[] scopes = _configuration.GetValue<string>("Graph:Scopes")?.Split(' ');
            var graphServiceClient = GetGraphServiceClient(scopes);

            var result = new List<CalendarEntry>();

            var calendarNames = _configuration.GetSection("Calendar:CalendarNames").Get<string[]>();
            var filterClause = string.Join(" or ", calendarNames.Select(cn => "name eq '" + cn + "'"));

            var primaryCalendar = _configuration["Calendar:Primary"] ?? "Calendar";
            var timeZone = _configuration["Calendar:TimeZone"] ?? "UTC";

            var calendars = await graphServiceClient.Me.Calendars
                .Request()
                .Filter(filterClause)
                .GetAsync();

            var calendarQueryOptions = new List<Microsoft.Graph.QueryOption>()
                {
                    new Microsoft.Graph.QueryOption("startDateTime", startDate.ToString("s")),
                    new Microsoft.Graph.QueryOption("endDateTime", endDate.ToString("s"))
                };

            foreach (var calendar in calendars)
            {
                var calendarView = await graphServiceClient.Me.Calendars[calendar.Id]
                    .CalendarView
                    .Request(calendarQueryOptions)
                    .Header("Prefer", $"outlook.timezone=\"{timeZone}\"")
                    .GetAsync();

                var pageIterator = PageIterator<Event>.CreatePageIterator(
                    graphServiceClient, calendarView,
                    (e) =>
                    {
                        if ((bool)e.IsAllDay)
                        {
                            var currentDay = e.Start.ToDateTime();
                            while (currentDay.CompareTo(e.End.ToDateTime()) < 0)
                            {
                                result.Add(new CalendarEntry()
                                {
                                    Description = e.Subject,
                                    Date = currentDay.ToString("s").Substring(0, 10),
                                    Time = string.Empty,
                                    AllDayEvent = true,
                                    IsPrimary = calendar.Name.Equals(primaryCalendar),
                                    IsSecondary = !calendar.Name.Equals(primaryCalendar)
                                });
                                currentDay = currentDay.AddDays(1);
                            }
                        }
                        else
                        {
                            result.Add(new CalendarEntry()
                            {
                                Description = e.Subject,
                                Date = e.Start.DateTime.Substring(0, 10),
                                Time = e.Start.DateTime.Substring(11, 5),
                                AllDayEvent = false,
                                IsPrimary = calendar.Name.Equals(primaryCalendar),
                                IsSecondary = !calendar.Name.Equals(primaryCalendar)
                            });
                        }
                        return true;
                    },
                    (req) =>
                    {
                        // Re-add the header to subsequent requests
                        req.Header("Prefer", $"outlook.timezone=\"{timeZone}\"");
                        return req;
                    }
                );
                await pageIterator.IterateAsync();

            }

            return result;
        }

        private GraphServiceClient GetGraphServiceClient(string[] scopes)
        {
            return GraphServiceClientFactory.GetAuthenticatedGraphClient(async () =>
            {
                IConfidentialClientApplication app = GetConfidentialClientApplication();
                var account = await _msalAccountActivityStore.GetMsalAccountLastActivity();
                var token = await app.AcquireTokenSilent(scopes, new MsalAccount
                    {
                        HomeAccountId = new AccountId(
                                            account.AccountIdentifier,
                                            account.AccountObjectId,
                                            account.AccountTenantId)
                    })
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                return token.AccessToken;
            }, _configuration.GetValue<string>("Graph:BaseUrl"));
        }

        private IConfidentialClientApplication GetConfidentialClientApplication()
        {
            var config = new AuthenticationConfig();
            _configuration.GetSection("AzureAd").Bind(config);
            var app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                .WithClientSecret(config.ClientSecret)
                .WithAuthority(new Uri(config.Authority))
                .Build();

            _msalTokenCacheProvider.Initialize(app.UserTokenCache);

            return app;
        }
    }
}