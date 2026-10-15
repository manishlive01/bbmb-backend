namespace BBMB.BudgetService.Features.BudgetProcess.Settings.CreateDelegationLimit.Dtos
{
    /// <summary>Body of <c>POST /api/budget-settings/delegation-limits</c>: a new T-19 row (append-only, design §8.2).</summary>
    public sealed class CreateDelegationLimitDto
    {
        /// <summary>SECRETARY, CE, SE or EE.</summary>
        public string AuthorityLevel { get; set; } = string.Empty;
        /// <summary>Whole rupees, &gt; 0; null records that the level has no limit from this date.</summary>
        public decimal? MaxAmount { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        /// <summary>The order of delegation (R-06).</summary>
        public string SourceRef { get; set; } = string.Empty;
    }
}
