using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FamilyBoard.Core.Calendar
{
    public class SchoolHolidaysService : ICalendarService
    {
        private const string SCHOOLHOLIDAYSURL = "https://ferien-api.de/api/v1/holidays/BW";

        private readonly ILogger<SchoolHolidaysService> _logger;

        public SchoolHolidaysService(ILogger<SchoolHolidaysService> logger)
        {
            _logger = logger;
        }

        public bool IsHolidays => true;

        public string Name => nameof(SchoolHolidaysService);

        public async Task<List<CalendarEntry>> GetEvents(
            DateTime startDate,
            DateTime endDate,
            bool isPrimary = false,
            bool isSecondary = false
        )
        {
            var result = new List<CalendarEntry>();

            result.AddRange(await GetHolidaysForYear(startDate, endDate, startDate.Year));

            return result;
        }

        private async Task<List<CalendarEntry>> GetHolidaysForYear(
            DateTime startDate,
            DateTime endDate,
            int year
        )
        {
            var startDateISO = startDate
                .ToString("s", System.Globalization.CultureInfo.InvariantCulture)
                .Substring(0, 10);
            var endDateISO = endDate
                .ToString("s", System.Globalization.CultureInfo.InvariantCulture)
                .Substring(0, 10);

            var yearResult = new List<CalendarEntry>();

            try
            {
                using (var client = new HttpClient())
                {
                    var holidayRequest = new HttpRequestMessage(HttpMethod.Get, SCHOOLHOLIDAYSURL);
                    var holidayResponse = await client.SendAsync(holidayRequest);
                    if (holidayResponse.IsSuccessStatusCode)
                    {
                        var holidaysPayload = await holidayResponse.Content.ReadAsStringAsync();
                        var holidays = JsonDocument
                            .Parse(holidaysPayload)
                            .RootElement.EnumerateArray();

                        foreach (var holiday in holidays)
                        {
                            var startsOn = holiday.GetProperty("start").GetDateTime();
                            var endsOn = holiday.GetProperty("end").GetDateTime();
                            var name = holiday.GetProperty("name").GetString();

                            var duration = endsOn - startsOn;

                            if (
                                startsOn.CompareTo(endDate) <= 0
                                && endsOn.CompareTo(startDate) >= 0
                                && duration.CompareTo(new TimeSpan(0)) > 0
                                && name.Length > 1
                            )
                            {
                                var date = startsOn;
                                while (date <= endsOn)
                                {
                                    yearResult.Add(
                                        new CalendarEntry()
                                        {
                                            AllDayEvent = true,
                                            SchoolHoliday = true,
                                            Date = date.ToString("u").Substring(0, 10),
                                            Description =
                                                name.Substring(0, 1).ToUpper() + name.Substring(1),
                                        }
                                    );
                                    ;
                                    date = date.AddDays(1);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetHolidaysForYear");
            }

            return yearResult.GroupBy(x => x.Date).Select(y => y.First()).ToList<CalendarEntry>();
        }
    }
}
