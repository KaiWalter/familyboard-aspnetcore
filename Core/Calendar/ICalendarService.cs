using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FamilyBoard.Core.Calendar
{
    public interface ICalendarService
    {
        Task<List<CalendarEntry>> GetEvents(DateTime startDate, DateTime endDate, bool isPrimary = false, bool isSecondary = false);
        bool IsHolidays { get; }
        string Name { get; }
    }
}