using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.BudgetProcess.Recommendations.Entities
{
    /// <summary>
    /// V-03 <c>budget.v_demand_by_ce</c> (DDD §8, BUD-302, BUD-304): for head-wise categories, the totals of
    /// columns 7 and 8 by cycle, CE and account code, over the non-deleted HEAD lines of the offices under the CE.
    /// </summary>
    public class DemandByCeRow
    {
        [Column("cycle_id")] public long CycleId { get; set; }
        [Column("category")] public string Category { get; set; } = string.Empty;
        [Column("ce_office_id")] public long CeOfficeId { get; set; }
        [Column("account_code_id")] public long AccountCodeId { get; set; }
        [Column("rbe_total")] public decimal? RbeTotal { get; set; }
        [Column("obe_next_total")] public decimal? ObeNextTotal { get; set; }
        [Column("line_count")] public int LineCount { get; set; }
        [Column("office_count")] public int OfficeCount { get; set; }
    }
}
