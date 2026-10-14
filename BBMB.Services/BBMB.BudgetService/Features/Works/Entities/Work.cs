using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.SubMinor.Entities;
using BBMB.BudgetService.Features.Budgets.Entities;
using BBMB.BudgetService.Features.Designations.Entities;
using BBMB.BudgetService.Features.DetailHeads.Entities;
using BBMB.BudgetService.Features.HierarchyManagement.Divisions.Entities;
using BBMB.BudgetService.Features.ObjectClassifications.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.Works.Entities
{
    [Table("work")]
    public class Work
    {
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(8)]
        [Column("work_code")]
        public int WorkCode { get; set; } // business key

        [Column("division_id")]
        public int DivisionId { get; set; } // ✅ FK
        [Column("div_section_id")]
        public int? DivSectionId { get; set; } // optional 

        [Required]
        [MaxLength(4)]
        [Column("major_acc_code")]
        public string MajorAccCode { get; set; }

        [Required]
        [MaxLength(2)]
        [Column("sub_major_acc_code")]
        public string SubMajorAccCode { get; set; }

        [Required]
        [MaxLength(3)]
        [Column("minor_acc_code")]
        public string MinorAccCode { get; set; }

        [Required]
        [MaxLength(2)]
        [Column("sub_minor_acc_code")]
        public string SubMinorAccCode { get; set; }

        [Required]
        [MaxLength(3)]
        [Column("detail_acc_code")]
        public string DetailAccCode { get; set; }

        [Required]
        [MaxLength(3)]
        [Column("object_acc_code")]
        public string ObjectAccCode { get; set; }

 

        // D-5: amounts held as text. Not converted (design §4.5); the budget process never reads them. Removal is OI-10.
        [Obsolete("D-5: text amount, not read by the budget process; removal is OI-10.")]
        [Column("original_budget")]
        public string OriginalBudget { get; set; } = string.Empty;

        [Obsolete("D-5: text amount, not read by the budget process; removal is OI-10.")]
        [Column("revised_budget")]
        public string RevisedBudget { get; set; } = string.Empty;

        [Obsolete("D-5: text amount, not read by the budget process; removal is OI-10.")]
        [Column("proposed_budget")]
        public string ProposedBudget { get; set; } = string.Empty;

        [Column("remarks")]
        public string Remarks { get; set; } = string.Empty;

        [MaxLength(4)]
        [Column("fin_yr_start")]
        public string? FinYrStart { get; set; }

        [MaxLength(4)]
        [Column("fin_yr_end")]
        public string? FinYrEnd { get; set; }

        [MaxLength(20)]
        [Column("sanction_no")]
        public string? SanctionNo { get; set; }

        [MaxLength(11)]
        [Column("sanction_date")]
        public string? SanctionDate { get; set; }

        [MaxLength(6)]
        [Column("designation_code")]
        public string? DesignationCode { get; set; }

        // Column type budget.rupees, configured in BudgetContext (design §4.5).
        [Column("estimated_cost")]
        public decimal? EstimatedCost { get; set; }

        [MaxLength(1)]
        [Column("status")]
        public string? Status { get; set; }

        // DDD T-23: varchar(500) (design §5.2).
        [Required]
        [MaxLength(500)]
        [Column("work_desc")]
        public string WorkDesc { get; set; } = string.Empty;

        // ---------------- DDD T-23 columns added in P1 (design §5.2, §5.5 row a) ----------------

        /// <summary>AA or AMR; fixes the ceiling. Required for every row with pre_ddd = false (CHECK).</summary>
        [MaxLength(3)]
        [Column("work_class")]
        public string? WorkClass { get; set; }

        /// <summary>F-07 column 14: the AM&amp;R work this work continues. Only for AMR (CHECK).</summary>
        [Column("preceding_amr_work_id")]
        public int? PrecedingAmrWorkId { get; set; }

        /// <summary>
        /// How the work entered (DDD T-23). Set server-side, never from the request body (design §9.3, §11.3).
        /// Required for every row with pre_ddd = false (CHECK).
        /// </summary>
        [MaxLength(16)]
        [Column("entered_via")]
        public string? EnteredVia { get; set; }

        /// <summary>
        /// True for rows that existed before the DDD (§5.5 row a). One-way true → false
        /// (trg_work_pre_ddd_one_way), cleared only in the first cycle.
        /// </summary>
        [Column("pre_ddd")]
        public bool PreDdd { get; set; }

        // [A] actor columns. Creation columns nullable, never backfilled (an old work's creator was not
        // recorded), always filled on insert and immutable on update by fn_require_actor.
        [Column("created_at")]
        public DateTimeOffset? CreatedAt { get; private set; }

        [Column("created_by")]
        public Guid? CreatedBy { get; private set; }

        [Column("created_charge_id")]
        public Guid? CreatedChargeId { get; private set; }

        [Column("updated_at")]
        public DateTimeOffset? UpdatedAt { get; private set; }

        [Column("updated_by")]
        public Guid? UpdatedBy { get; private set; }

        [Column("updated_charge_id")]
        public Guid? UpdatedChargeId { get; private set; }

        [MaxLength(1)]
        [Column("nature")]
        public string? Nature { get; set; }

        [MaxLength(8)]
        [Column("ref_work_code")]
        public string? RefWorkCode { get; set; }

        [Required]
        [MaxLength(6)]
        [Column("user_code")]
        public string UserCode { get; set; } = string.Empty;

        // Navigation properties for foreign keys
        public virtual Division? Division { get; set; }
        public virtual MajorHead? MajorHead { get; set; }
        public virtual SubMajorHead? SubMajorHead { get; set; }
        public virtual MinorHead? MinorHead { get; set; }
        public virtual SubMinorHead? SubMinorHead { get; set; }
        public virtual DetailHead? DetailHead { get; set; }
        public virtual ObjectClassification? ObjectClassification { get; set; }
        public virtual Designation? Designation { get; set; }
        public ICollection<BudgetEntry> BudgetEntries { get; set; } = new List<BudgetEntry>();

    }

    /// <summary>T-23 <c>work_class</c> values (CHECK).</summary>
    public static class WorkClass
    {
        public const string Aa = "AA";
        public const string Amr = "AMR";
    }

    /// <summary>T-23 <c>entered_via</c> values (CHECK).</summary>
    public static class WorkEnteredVia
    {
        public const string AaEntry = "AA_ENTRY";
        public const string DemandEntry = "DEMAND_ENTRY";
        public const string TsEntry = "TS_ENTRY";
        public const string ReappropNew = "REAPPROP_NEW";
        public const string LegacyOpening = "LEGACY_OPENING";
    }
}
