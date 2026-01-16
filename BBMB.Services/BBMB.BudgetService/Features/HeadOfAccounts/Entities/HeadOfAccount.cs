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
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(4)]
        [Column("major_acc_code_fk")]
        public string MajorAccCode { get; set; } = string.Empty;

        [MaxLength(4)]
        [Column("sub_major_acc_code_fk")]
        public string? SubMajorAccCode { get; set; }

        [Required]
        [MaxLength(3)]
        [Column("minor_acc_code_fk")]
        public string MinorAccCode { get; set; } = string.Empty;

        [MaxLength(2)]
        [Column("sub_minor_acc_code_fk")]
        public string? SubMinorAccCode { get; set; }

        [Required]
        [MaxLength(3)]
        [Column("detail_acc_code_fk")]
        public string DetailAccCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(3)]
        [Column("object_acc_code_fk")]
        public string ObjectAccCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [Column("head_desc")]
        public string HeadDesc { get; set; } = string.Empty;

        [MaxLength(1)]
        [Column("party_emp_required")]
        public string? PartyEmpRequired { get; set; }

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

        // Navigation properties for foreign keys
        public virtual MajorHead? MajorHead { get; set; }
        public virtual MinorHead? MinorHead { get; set; }
        public virtual DetailHead? DetailHead { get; set; }
        public virtual ObjectClassification? ObjectClassification { get; set; }

        // Computed property for full account code
        [NotMapped]
        public string FullAccountCode =>
            $"{MajorAccCode}{SubMajorAccCode ?? ""}{MinorAccCode}{SubMinorAccCode ?? ""}{DetailAccCode}{ObjectAccCode}";
    }
}
