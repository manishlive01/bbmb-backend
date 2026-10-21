namespace BBMB.BudgetService.Features.BudgetProcess.Distributions.Rules
{
    /// <summary>
    /// VR-07 / DB-18 (design §7, §8.5): the distribution of a recommendation totals, column by column, no more
    /// than the figure recommended (M-14, BUD-157). A null recommended figure is treated as nil — nothing may be
    /// distributed from an unset figure. This is the application pre-check; the database re-checks the same rule
    /// as a set at commit (the DB-18 deferred constraint trigger), so a concurrent write cannot slip past.
    /// </summary>
    public static class DistributionRules
    {
        /// <summary>True when neither column's total exceeds the recommended figure (VR-07 holds).</summary>
        public static bool WithinCeiling(
            decimal? rbeRecommended, decimal? obeNextRecommended, decimal rbeTotal, decimal obeTotal) =>
            rbeTotal <= (rbeRecommended ?? 0m) && obeTotal <= (obeNextRecommended ?? 0m);

        /// <summary>The column that is over, for the M-14 message, or null when within the ceiling.</summary>
        public static string? OverColumn(
            decimal? rbeRecommended, decimal? obeNextRecommended, decimal rbeTotal, decimal obeTotal)
        {
            if (rbeTotal > (rbeRecommended ?? 0m))
                return "rbeAmount";
            if (obeTotal > (obeNextRecommended ?? 0m))
                return "obeNextAmount";
            return null;
        }
    }
}
