namespace BBMB.BudgetService.Features.BudgetProcess.Reports.GetConsolidation.Dtos
{
    /// <summary>
    /// Design §8.8 (F-18). Consolidated totals of the demanded figures grouped by a hierarchy key. The allocated
    /// and frozen totals are reported as <c>allocatedAvailable = false</c> rather than a number: those figures
    /// come from allocation/freeze (T-40/T-44), delivered in P4, which is deferred (user time-box 2026-10-15).
    /// Never a fabricated zero.
    /// </summary>
    public sealed record ConsolidationResultDto(
        string GroupBy,
        bool AllocatedAvailable,
        string AllocatedNote,
        IReadOnlyList<ConsolidationRowDto> Rows);

    public sealed record ConsolidationRowDto(
        string Key,
        string? Label,
        string LineClass,
        decimal DemandedRbe,
        decimal DemandedObeNext,
        decimal DistributedRbe,
        decimal DistributedObeNext);
}
