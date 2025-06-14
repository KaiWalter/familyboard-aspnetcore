using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using FamilyBoard.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FamilyBoard.Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class CalendarController : ControllerBase
    {
        private readonly ILogger<CalendarController> _logger;

        private readonly IConfiguration _configuration;

        private readonly IEnumerable<ICalendarService> _calendarServices;

        public CalendarController(
            ILogger<CalendarController> logger,
            IConfiguration configuration,
            IEnumerable<ICalendarService> calendarServices
        )
        {
            _logger = logger;
            _configuration = configuration;
            _calendarServices = calendarServices;
        }

        /// <summary>
        /// Retrieves all calendar entries.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(
            typeof(List<CalendarEntry>),
            StatusCodes.Status200OK,
            "application/json"
        )]
        public async Task<ActionResult<List<CalendarEntry>>> GetCalendarEntries()
        {
            _logger.LogTrace("REQUEST:" + nameof(GetCalendarEntries));

            var result = new List<CalendarEntry>();

            var startTime = DateTime.Today.AddDays(-7);
            var endTime = DateTime.Now.AddDays(21);

            foreach (var calendarService in _calendarServices)
            {
                var serviceResult = await calendarService.GetEvents(
                    startTime,
                    endTime,
                    false,
                    false
                );
                result.AddRange(serviceResult);
            }

            return Ok(result);
        }

        /// <summary>
        /// Retrieves month and weekday names for configured culture.
        /// </summary>
        [HttpGet("dateformatinfo")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(DateFormatInfo), StatusCodes.Status200OK, "application/json")]
        public ActionResult<DateFormatInfo> GetDateFormatInfo()
        {
            _logger.LogTrace("REQUEST:" + nameof(GetDateFormatInfo));

            var ci = new CultureInfo(_configuration["Calendar:Culture"] ?? "de-DE");
            var dtf = ci.DateTimeFormat;

            var result = new DateFormatInfo
            {
                MonthNames = new List<string>(12),
                WeekDayNames = new List<string>(7),
            };

            for (var m = 1; m <= 12; m++)
            {
                result.MonthNames.Add(dtf.GetAbbreviatedMonthName(m));
            }

            result.WeekDayNames.Add(dtf.GetAbbreviatedDayName(System.DayOfWeek.Monday));
            result.WeekDayNames.Add(dtf.GetAbbreviatedDayName(System.DayOfWeek.Tuesday));
            result.WeekDayNames.Add(dtf.GetAbbreviatedDayName(System.DayOfWeek.Wednesday));
            result.WeekDayNames.Add(dtf.GetAbbreviatedDayName(System.DayOfWeek.Thursday));
            result.WeekDayNames.Add(dtf.GetAbbreviatedDayName(System.DayOfWeek.Friday));
            result.WeekDayNames.Add(dtf.GetAbbreviatedDayName(System.DayOfWeek.Saturday));
            result.WeekDayNames.Add(dtf.GetAbbreviatedDayName(System.DayOfWeek.Sunday));

            return result;
        }
    }
}
