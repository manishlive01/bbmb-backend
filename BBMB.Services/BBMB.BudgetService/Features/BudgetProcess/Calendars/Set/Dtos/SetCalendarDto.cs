namespace BBMB.BudgetService.Features.BudgetProcess.Calendars.Set.Dtos
{
    /// <summary>Design §8.5 <c>PUT /api/budget-cycles/{cycleId}/calendar</c> body (F-23, BUD-213).</summary>
    public sealed class SetCalendarDto
    {
        public List<CalendarEntryDto> Entries { get; set; } = [];
    }

    /// <summary>One last date for an office level (BUD-210, BUD-213).</summary>
    public sealed class CalendarEntryDto
    {
        /// <summary>A T-13 office level save IT_CELL (DIVISION, CIRCLE, CE, SECRETARIAT, FINANCE).</summary>
        public string OfficeLevel { get; set; } = string.Empty;
        public DateOnly LastDate { get; set; }
    }
}
