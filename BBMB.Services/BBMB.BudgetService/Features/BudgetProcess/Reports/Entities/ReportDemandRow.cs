using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.BudgetProcess.Reports.Entities
{
    /// <summary>
    /// <c>budget.v_report_demand</c> (design §5.4, §8.8 F-26): the P4/P5-independent reporting fact — one row per
    /// non-deleted demand line with its proposal, cycle, framing-office hierarchy keys and head, the demanded
    /// figures and the distributed share for the framing office. Keyless. Allocation/freeze/re-appropriation
    /// columns are deliberately absent (they arrive with P6-01b over the P4/P5 tables).
    /// </summary>
    public class ReportDemandRow
    {
        [Column("demand_line_id")] public long DemandLineId { get; set; }
        [Column("proposal_id")] public long ProposalId { get; set; }
        [Column("cycle_id")] public long CycleId { get; set; }
        [Column("current_fin_year")] public int CurrentFinYear { get; set; }
        [Column("category")] public string Category { get; set; } = string.Empty;
        [Column("state_code")] public string StateCode { get; set; } = string.Empty;
        [Column("origin_office_id")] public long OriginOfficeId { get; set; }
        [Column("origin_office_level")] public string OriginOfficeLevel { get; set; } = string.Empty;
        [Column("wing_code")] public string? WingCode { get; set; }
        [Column("ce_id")] public int? CeId { get; set; }
        [Column("circle_id")] public int? CircleId { get; set; }
        [Column("division_id")] public int? DivisionId { get; set; }
        [Column("line_type")] public string LineType { get; set; } = string.Empty;
        [Column("work_id")] public int? WorkId { get; set; }
        [Column("account_code_id")] public long? AccountCodeId { get; set; }
        [Column("complete_code")] public string? CompleteCode { get; set; }
        [Column("head_desc")] public string? HeadDesc { get; set; }
        [Column("demanded_rbe")] public decimal? DemandedRbe { get; set; }
        [Column("demanded_obe_next")] public decimal? DemandedObeNext { get; set; }
        [Column("distributed_rbe")] public decimal? DistributedRbe { get; set; }
        [Column("distributed_obe_next")] public decimal? DistributedObeNext { get; set; }
    }
}
