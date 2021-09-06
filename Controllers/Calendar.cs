using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FamilyBoard.Models;
using System.Collections.Generic;
using System;

namespace FamilyBoard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<CalendarEntry>>> GetCalendarEntries(int id)
        {
            var result = new List<CalendarEntry>();

            result.Add(new CalendarEntry()
            {
                Description = "Test",
                Date = DateTime.Now,
                Time = "",
                IsPrimary = true
            });

            return Ok(result);
        }
    }
}

