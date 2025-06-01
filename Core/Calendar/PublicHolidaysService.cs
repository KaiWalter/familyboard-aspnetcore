using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FamilyBoard.Core.Calendar
{
    public class PublicHolidaysService : ICalendarService
    {
        private const string PUBLICHOLIDAYSURL =
            "https://feiertage-api.de/api/?nur_land=BW&jahr={0}";

        private readonly ILogger<PublicHolidaysService> _logger;

        public PublicHolidaysService(ILogger<PublicHolidaysService> logger)
        {
            _logger = logger;
        }

        public bool IsHolidays => true;

        public string Name => nameof(PublicHolidaysService);

        public async Task<List<CalendarEntry>> GetEvents(
            DateTime startDate,
            DateTime endDate,
            bool isPrimary = false,
            bool isSecondary = false
        )
        {
            if ((endDate.Year - startDate.Year) > 1)
            {
                throw new ArgumentException(
                    $"maximum span of years between {nameof(startDate)} and {nameof(endDate)} is 2"
                );
            }

            var result = new List<CalendarEntry>();

            result.AddRange(await GetHolidaysForYear(startDate, endDate, startDate.Year));
            if (endDate.Year != startDate.Year)
            {
                result.AddRange(await GetHolidaysForYear(startDate, endDate, endDate.Year));
            }

            return result;
        }

        private static async Task<List<CalendarEntry>> GetHolidaysForYear(
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

            using (var client = new HttpClient())
            {
                var holidayRequest = new HttpRequestMessage(
                    HttpMethod.Get,
                    string.Format(PUBLICHOLIDAYSURL, year)
                );
                var holidayResponse = await client.SendAsync(holidayRequest);
                if (holidayResponse.IsSuccessStatusCode)
                {
                    var holidaysPayload = await holidayResponse.Content.ReadAsStringAsync();
                    var holidays = JsonDocument
                        .Parse(holidaysPayload)
                        .RootElement.EnumerateObject();
                    foreach (var holiday in holidays)
                    {
                        var day = holiday.Value.GetProperty("datum").GetString();
                        if (day.CompareTo(startDateISO) >= 0 && day.CompareTo(endDateISO) <= 0)
                        {
                            yearResult.Add(
                                new CalendarEntry()
                                {
                                    AllDayEvent = true,
                                    PublicHoliday = true,
                                    Description = holiday.Name,
                                    Date = day,
                                }
                            );
                        }
                    }
                }
            }

            return yearResult;
        }
    }
}
