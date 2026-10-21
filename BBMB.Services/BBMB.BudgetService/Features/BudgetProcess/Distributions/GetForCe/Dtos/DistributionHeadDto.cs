namespace BBMB.BudgetService.Features.BudgetProcess.Distributions.GetForCe.Dtos
{
    /// <summary>Design §8.5: one head (recommendation) with its recommended figure, its shares and its balance.</summary>
    public sealed record DistributionHeadDto(
        long RecommendationId,
        long AccountCodeId,
        string? CompleteCode,
        decimal RbeRecommended,
        decimal ObeNextRecommended,
        decimal RbeDistributed,
        decimal ObeDistributed,
        decimal RbeBalanceRetained,
        decimal ObeBalanceRetained,
        bool RedistributionRequired,
        IReadOnlyList<DistributionShareDto> Shares);

    /// <summary>One office's share of a head (column 9/10 as distributed).</summary>
    public sealed record DistributionShareDto(long OfficeId, decimal RbeAmount, decimal ObeNextAmount);
}
