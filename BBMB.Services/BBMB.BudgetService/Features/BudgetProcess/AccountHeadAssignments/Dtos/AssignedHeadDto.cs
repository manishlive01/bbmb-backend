namespace BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.Dtos
{
    /// <summary>A live T-21 row with its head (F-17 list; the heads offered at F-11, VR-06).</summary>
    public sealed record AssignedHeadDto(
        long AssignmentId, long OfficeId, long AccountCodeId, string CompleteCode, string HeadDesc, string Source, bool NotForUse);
}
