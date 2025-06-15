using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FamilyBoard.Core.Graph;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Extensions;

namespace FamilyBoard.Core.Calendar
{
    public class OutlookService : ICalendarService
    {
        private readonly ILogger<OutlookService> _logger;
        private readonly IGraphService _graphService;
        private readonly IConfiguration _configuration;

        public OutlookService(
            ILogger<OutlookService> logger,
            IConfiguration configuration,
            IGraphService graphService
        )
        {
            _logger = logger;
            _configuration = configuration;
            _graphService = graphService;
        }

        public bool IsHolidays => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public async Task<List<CalendarEntry>> GetEvents(
            DateTime startDate,
            DateTime endDate,
            bool isPrimary = false,
            bool isSecondary = false
        )
        {
            var graphServiceClient = _graphService.GetGraphServiceClient();

            var result = new List<CalendarEntry>();

            var calendarNames = _configuration.GetSection("Calendar:CalendarNames").Get<string[]>();
            var filterClause = string.Join(
                " or ",
                calendarNames.Select(cn => "name eq '" + cn + "'")
            );

            var primaryCalendar = _configuration["Calendar:Primary"] ?? "Calendar";
            var timeZone = _configuration["Calendar:TimeZone"] ?? "UTC";

            try
            {
                var calendars = await graphServiceClient
                    .Me.Calendars.Request()
                    .Select("id,name")
                    .Filter(filterClause)
                    .GetAsync();

                var calendarQueryOptions = new List<Microsoft.Graph.QueryOption>()
                {
                    new Microsoft.Graph.QueryOption("startDateTime", startDate.ToString("s")),
                    new Microsoft.Graph.QueryOption("endDateTime", endDate.ToString("s")),
                };

                foreach (var calendar in calendars)
                {
                    var calendarView = await graphServiceClient
                        .Me.Calendars[calendar.Id]
                        .CalendarView.Request(calendarQueryOptions)
                        .Header("Prefer", $"outlook.timezone=\"{timeZone}\"")
                        .GetAsync();

                    var pageIterator = PageIterator<Event>.CreatePageIterator(
                        graphServiceClient,
                        calendarView,
                        (e) =>
                        {
                            if ((bool)e.IsAllDay)
                            {
                                var currentDay = e.Start.ToDateTime();
                                while (currentDay.CompareTo(e.End.ToDateTime()) < 0)
                                {
                                    result.Add(
                                        new CalendarEntry()
                                        {
                                            Description = e.Subject,
                                            Date = currentDay.ToString("s").Substring(0, 10),
                                            Time = string.Empty,
                                            AllDayEvent = true,
                                            IsPrimary = calendar.Name.Equals(primaryCalendar),
                                            IsSecondary = !calendar.Name.Equals(primaryCalendar),
                                        }
                                    );
                                    currentDay = currentDay.AddDays(1);
                                }
                            }
                            else
                            {
                                result.Add(
                                    new CalendarEntry()
                                    {
                                        Description = e.Subject,
                                        Date = e.Start.DateTime.Substring(0, 10),
                                        Time = e.Start.DateTime.Substring(11, 5),
                                        AllDayEvent = false,
                                        IsPrimary = calendar.Name.Equals(primaryCalendar),
                                        IsSecondary = !calendar.Name.Equals(primaryCalendar),
                                    }
                                );
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving calendar events from Outlook");
                result.Add(
                    new CalendarEntry()
                    {
                        Description = $"Outlook:{ex.Message}",
                        Date = DateTime.UtcNow.ToString("s").Substring(0, 10),
                        Time = DateTime.UtcNow.ToString("s").Substring(11, 5),
                        AllDayEvent = false,
                        IsPrimary = false,
                        IsSecondary = false,
                    }
                );
            }

            result.Sort((x, y) => (x.Date + x.Time).CompareTo(y.Date + y.Time));

            return result;
        }
    }
}
