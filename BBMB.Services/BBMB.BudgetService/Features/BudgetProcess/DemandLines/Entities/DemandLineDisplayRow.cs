using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.Entities
{
    /// <summary>
    /// V-04 <c>budget.v_demand_line_display</c>, version 1 (design §5.4): a demand line with the displayed
    /// columns joined (work, account code, opening figures, expenditure) and the figure at each level from
    /// T-31. Keyless; grows by CREATE OR REPLACE VIEW in P2, P3 and P4.
    /// </summary>
    public class DemandLineDisplayRow
    {
        [Column("demand_line_id")] public long DemandLineId { get; set; }
        [Column("proposal_id")] public long ProposalId { get; set; }
        [Column("cycle_id")] public long CycleId { get; set; }
        [Column("category")] public string Category { get; set; } = string.Empty;
        [Column("origin_office_id")] public long OriginOfficeId { get; set; }
        [Column("serial_no")] public int SerialNo { get; set; }
        [Column("line_type")] public string LineType { get; set; } = string.Empty;
        [Column("work_id")] public int? WorkId { get; set; }
        [Column("work_desc")] public string? WorkDesc { get; set; }
        [Column("work_class")] public string? WorkClass { get; set; }
        [Column("preceding_amr_work_id")] public int? PrecedingAmrWorkId { get; set; }
        [Column("account_code_id")] public long? AccountCodeId { get; set; }
        [Column("complete_code")] public string? CompleteCode { get; set; }
        [Column("head_desc")] public string? HeadDesc { get; set; }
        [Column("unforeseen_name")] public string? UnforeseenName { get; set; }
        [Column("rbe_amount")] public decimal? RbeAmount { get; set; }
        [Column("obe_next_amount")] public decimal? ObeNextAmount { get; set; }
        [Column("justification_text")] public string? JustificationText { get; set; }
        [Column("pct_check_not_applied")] public bool PctCheckNotApplied { get; set; }
        [Column("floor_check_unchecked")] public bool FloorCheckUnchecked { get; set; }
        [Column("added_on_return")] public bool AddedOnReturn { get; set; }
        [Column("is_deleted")] public bool IsDeleted { get; set; }
        /// <summary>T-36 OBE_CURRENT (first cycle).</summary>
        [Column("opening_obe_current")] public decimal? OpeningObeCurrent { get; set; }
        /// <summary>T-36 RBE_PREVIOUS (first cycle).</summary>
        [Column("opening_rbe_previous")] public decimal? OpeningRbePrevious { get; set; }
        /// <summary>T-35 current year, latest of UP_TO_DATE / UP_TO_MONTH (keyed preferred, BUD-025).</summary>
        [Column("expenditure_to_date")] public decimal? ExpenditureToDate { get; set; }
        /// <summary>T-35 previous year, FULL_YEAR.</summary>
        [Column("expenditure_previous_year")] public decimal? ExpenditurePreviousYear { get; set; }
        /// <summary>
        /// JSON array of the figure at each level office from T-31:
        /// [{"level_office_id":…,"figure":"RBE"|"OBE_NEXT","value":…}], latest value per level and figure.
        /// </summary>
        [Column("level_figures")] public string LevelFigures { get; set; } = "[]";
    }
}
