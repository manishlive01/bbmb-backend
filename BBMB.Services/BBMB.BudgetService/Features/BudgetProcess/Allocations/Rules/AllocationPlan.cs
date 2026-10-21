namespace BBMB.BudgetService.Features.BudgetProcess.Allocations.Rules
{
    /// <summary>
    /// One provision the allocation would create, with its ALLOCATION movement amount. Computed once by
    /// <see cref="AllocationRules"/> and used both to show the preview (BUD-189) and to write the allocation,
    /// so the preview reflects exactly what the allocation commits.
    /// </summary>
    public sealed record PlannedProvision(
        string HolderType,          // WORK / HEAD / UNFORESEEN
        string Basis,               // RBE / OBE
        string Category,
        int FinYear,
        int? WorkId,
        long OfficeId,
        long? AccountCodeId,
        long? DemandLineId,
        long? RecommendationId,
        long? DistributionId,
        decimal Amount,
        bool BudgetaryCheck,
        long ProposalId)            // the proposal whose activation this provision belongs to
    {
        /// <summary>A WORK provision whose work has no code yet receives one on allocation (BUD-187, BUD-286).</summary>
        public bool NeedsWorkCode => HolderType == ProvisionNeedsCode.Work && WorkId is not null;
    }

    internal static class ProvisionNeedsCode
    {
        public const string Work = "WORK";
    }

    /// <summary>
    /// The full plan for a body (BSC or FULL_BOARD): the provisions to create and the proposals to transition.
    /// <see cref="LinesToActivate"/> and <see cref="TotalToActivate"/> are the DB-20 counts shown before
    /// confirmation and checked at allocation.
    /// </summary>
    public sealed record AllocationComputation(
        IReadOnlyList<PlannedProvision> Provisions,
        IReadOnlyList<long> ProposalIds,
        IReadOnlyList<int> WorksToReceiveCode)
    {
        public int LinesToActivate => Provisions.Count;
        public decimal TotalToActivate => Provisions.Sum(p => p.Amount);
    }
}
