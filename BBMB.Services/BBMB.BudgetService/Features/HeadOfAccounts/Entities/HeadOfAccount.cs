using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Entities;
using BBMB.BudgetService.Features.DetailHeads.Entities;
using BBMB.BudgetService.Features.ObjectClassifications.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.HeadOfAccounts.Entities
{
    [Table("budget_head_master")]
    public class HeadOfAccount
    {
        // 🔹 COMPOSITE PRIMARY KEY FIELDS

        [Required]
        [MaxLength(4)]
        [Column("major_acc_code")]
        public string MajorAccCode { get; set; } = string.Empty;

        // DDD T-07: varchar(2). Narrowed by BudgetP1ProposalSpineMasters after a guard (design §5.2).
        [MaxLength(2)]
        [Column("sub_major_acc_code")]
        public string? SubMajorAccCode { get; set; }

        [Required]
        [MaxLength(3)]
        [Column("minor_acc_code")]
        public string MinorAccCode { get; set; } = string.Empty;

        [MaxLength(2)]
        [Column("sub_minor_acc_code")]
        public string? SubMinorAccCode { get; set; }

        [Required]
        [MaxLength(3)]
        [Column("detail_acc_code")]
        public string DetailAccCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(3)]
        [Column("object_acc_code")]
        public string ObjectAccCode { get; set; } = string.Empty;

        // 🔹 OTHER COLUMNS

        [Required]
        [MaxLength(150)]
        [Column("head_desc")]
        public string HeadDesc { get; set; } = string.Empty;

        /// <summary>
        /// DDD T-07: NOT NULL, CHECK Y/N. Y on creation where the major head is 8443 or 9906 (BUD-267, ACC-009).
        /// </summary>
        [Required]
        [MaxLength(1)]
        [Column("party_emp_required")]
        public string PartyEmpRequired { get; set; } = "N";

        [MaxLength(1)]
        [Column("party_ledg_to_print")]
        public string? PartyLedgToPrint { get; set; }

        [MaxLength(1)]
        [Column("tds_deducted")]
        public string? TdsDeducted { get; set; }

        [MaxLength(1)]
        [Column("advance_head")]
        public string? AdvanceHead { get; set; }

        [MaxLength(1)]
        [Column("bank_cash")]
        public string? BankCash { get; set; }

        [MaxLength(1)]
        [Column("monthly_carry_fwd")]
        public string? MonthlyCarryFwd { get; set; }

        [Required]
        [Column("modification_date")]
        public DateTime ModificationDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(6)]
        [Column("user_code")]
        public string UserCode { get; set; } = string.Empty;

        // 🔹 Navigation Properties
        // ---------------- DDD T-07 columns added in P1 (design §5.2) ----------------

        /// <summary>GENERATED ALWAYS AS IDENTITY, UK. Every account_code_id of the process tables refers to it.</summary>
        [Column("account_code_id")]
        public long AccountCodeId { get; private set; }

        /// <summary>GENERATED ALWAYS AS the six segments joined by hyphens, STORED, UK (BUD-041, BUD-042).</summary>
        [MaxLength(22)]
        [Column("complete_code")]
        public string CompleteCode { get; private set; } = string.Empty;

        /// <summary>CREATED (at F-01) or LEGACY (loaded from the Manual in use, DDD §9 L-04).</summary>
        [Required]
        [MaxLength(7)]
        [Column("origin")]
        public string Origin { get; set; } = HeadOfAccountOrigin.Legacy;

        /// <summary>Required where origin is CREATED (BUD-044, M-03).</summary>
        [MaxLength(50)]
        [Column("eoffice_approval_ref")]
        public string? EofficeApprovalRef { get; set; }

        /// <summary>One-way false → true (DB-03, BUD-308).</summary>
        [Column("not_for_use")]
        public bool NotForUse { get; set; }

        [MaxLength(50)]
        [Column("not_for_use_ref")]
        public string? NotForUseRef { get; set; }

        [Column("not_for_use_at")]
        public DateTimeOffset? NotForUseAt { get; set; }

        [Column("not_for_use_by")]
        public Guid? NotForUseBy { get; set; }

        /// <summary>Creation actor columns: nullable on T-07, required where origin is CREATED; trigger-filled.</summary>
        [Column("created_at")]
        public DateTimeOffset? CreatedAt { get; private set; }

        [Column("created_by")]
        public Guid? CreatedBy { get; private set; }

        [Column("created_charge_id")]
        public Guid? CreatedChargeId { get; private set; }

        public virtual MajorHead? MajorHead { get; set; }
        public virtual MinorHead? MinorHead { get; set; }
        public virtual DetailHead? DetailHead { get; set; }
        public virtual ObjectClassification? ObjectClassification { get; set; }

        // 🔹 Computed
        [NotMapped]
        public string FullAccountCode =>
            $"{MajorAccCode}{SubMajorAccCode ?? ""}{MinorAccCode}{SubMinorAccCode ?? ""}{DetailAccCode}{ObjectAccCode}";
    }

    /// <summary>T-07 <c>origin</c> values (CHECK).</summary>
    public static class HeadOfAccountOrigin
    {
        public const string Created = "CREATED";
        public const string Legacy = "LEGACY";

        /// <summary>BUD-267: the party/employee flag is Y where the major head is 8443 or 9906, else N.</summary>
        public static string PartyEmpRequiredFor(string majorAccCode) =>
            majorAccCode is "8443" or "9906" ? "Y" : "N";
    }
}
