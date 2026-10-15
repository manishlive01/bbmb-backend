namespace BBMB.BudgetService.Features.BudgetProcess.Reports.GetDashboard.Dtos
{
    /// <summary>
    /// Design §8.8 (F-25, BUD-219). The actor's dashboard for a cycle: proposals by state per category (scoped),
    /// the proposals the actor's office holds with a link target, the actor-level calendar status, and (for
    /// Finance/Secretariat) the count of open correction flags (M-34).
    /// </summary>
    public sealed record DashboardDto(
        long CycleId,
        IReadOnlyList<CategoryStateCountDto> ByState,
        IReadOnlyList<PendingProposalDto> Pending,
        CalendarStatusDto Calendar,
        int? OpenCorrectionFlags);

    public sealed record CategoryStateCountDto(string Category, string StateCode, int Count);

    /// <summary>A proposal held by the actor's office, with the route and id to open it (BUD-219).</summary>
    public sealed record PendingProposalDto(long ProposalId, string Category, string StateCode, string LinkRoute);

    public sealed record CalendarStatusDto(string? ActorLevel, DateOnly? LastDate, int? DaysRemaining, bool Overdue);
}
