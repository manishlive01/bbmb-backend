namespace BBMB.BudgetService.Features.BudgetProcess.Reports.GetConsolidationComposition.Dtos
{
    /// <summary>Design §8.8 (BUD-178): one demand line contributing to a consolidation cell.</summary>
    public sealed record CompositionLineDto(
        long DemandLineId,
        long ProposalId,
        long OriginOfficeId,
        string LineType,
        int? WorkId,
        long? AccountCodeId,
        string? CompleteCode,
        decimal DemandedRbe,
        decimal DemandedObeNext);
}
