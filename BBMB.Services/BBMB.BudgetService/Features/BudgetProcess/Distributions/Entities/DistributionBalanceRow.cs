using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.BudgetProcess.Distributions.Entities
{
    /// <summary>
    /// V-05 <c>budget.v_distribution_balance</c> (design §5.4, DDD V-05, BUD-158, BUD-160): for each
    /// recommendation, the figure recommended, the total distributed and the balance retained at the CE office,
    /// column by column. Keyless; the balance is computed, never stored.
    /// </summary>
    public class DistributionBalanceRow
    {
        [Column("recommendation_id")] public long RecommendationId { get; set; }
        [Column("cycle_id")] public long CycleId { get; set; }
        [Column("category")] public string Category { get; set; } = string.Empty;
        [Column("ce_office_id")] public long CeOfficeId { get; set; }
        [Column("account_code_id")] public long AccountCodeId { get; set; }
        [Column("rbe_recommended")] public decimal RbeRecommended { get; set; }
        [Column("obe_next_recommended")] public decimal ObeNextRecommended { get; set; }
        [Column("rbe_distributed")] public decimal RbeDistributed { get; set; }
        [Column("obe_distributed")] public decimal ObeDistributed { get; set; }
        [Column("rbe_balance_retained")] public decimal RbeBalanceRetained { get; set; }
        [Column("obe_balance_retained")] public decimal ObeBalanceRetained { get; set; }
        [Column("redistribution_required")] public bool RedistributionRequired { get; set; }
    }
}
