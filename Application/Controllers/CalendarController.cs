using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Extensions;
using Microsoft.Identity.Web;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using FamilyBoard.Core.Calendar;

namespace FamilyBoard.Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        private readonly ILogger<CalendarController> _logger;

        private readonly GraphServiceClient _graphServiceClient;

        private readonly IConfiguration _configuration;

        private readonly IEnumerable<ICalendarService> _calendarServices;

        public CalendarController(ILogger<CalendarController> logger,
                            IConfiguration configuration,
                            GraphServiceClient graphServiceClient,
                            IEnumerable<ICalendarService> calendarServices)
        {
            _logger = logger;
            _graphServiceClient = graphServiceClient;
            _configuration = configuration;
            _calendarServices = calendarServices;
        }

        [HttpGet]
        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public async Task<ActionResult<List<CalendarEntry>>> GetCalendarEntries()
        {
            var result = new List<CalendarEntry>();

            var startTime = DateTime.Today.AddDays(-7);
            var endTime = DateTime.Now.AddDays(21);

            foreach(var calendarService in _calendarServices)
            {
                var serviceResult = await calendarService.GetEvents(startTime,endTime,false,false);
                result.AddRange(serviceResult);
            }

            return Ok(result);
        }
    }
}

