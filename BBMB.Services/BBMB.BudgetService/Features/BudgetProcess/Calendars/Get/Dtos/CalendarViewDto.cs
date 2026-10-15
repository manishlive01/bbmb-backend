namespace BBMB.BudgetService.Features.BudgetProcess.Calendars.Get.Dtos
{
    /// <summary>
    /// Design §8.5 Get (F-23, BUD-211): every calendar entry of the cycle, and for the actor's own level the
    /// pending count and the days remaining or overdue against the last date.
    /// </summary>
    public sealed record CalendarViewDto(
        long CycleId,
        IReadOnlyList<CalendarEntryViewDto> Entries,
        string? ActorLevel,
        DateOnly? ActorLastDate,
        int? DaysRemaining,
        bool Overdue,
        int PendingCount);

    public sealed record CalendarEntryViewDto(string OfficeLevel, DateOnly LastDate);
}
