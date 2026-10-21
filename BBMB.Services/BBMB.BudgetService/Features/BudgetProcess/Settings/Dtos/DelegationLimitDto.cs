namespace BBMB.BudgetService.Features.BudgetProcess.Settings.Dtos
{
    /// <summary>A T-19 row. <paramref name="MaxAmount"/> null means no limit.</summary>
    public sealed record DelegationLimitDto(
        long DelegationLimitId, string AuthorityLevel, decimal? MaxAmount, DateOnly EffectiveFrom, string SourceRef, bool InForce);
}
