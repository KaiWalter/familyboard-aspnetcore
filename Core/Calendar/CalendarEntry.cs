using System;

namespace FamilyBoard.Core.Calendar
{
    public class CalendarEntry
    {
        public string Description { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsSecondary { get; set; }
        public bool AllDayEvent { get; set; }
        public bool PublicHoliday { get; set; }
        public bool SchoolHoliday { get; set; }

    }
}