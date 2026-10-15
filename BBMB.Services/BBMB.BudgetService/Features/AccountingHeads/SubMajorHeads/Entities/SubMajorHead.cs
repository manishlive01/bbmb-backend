using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Entities
{
    [Table("fa_sub_major_head")]
    public class SubMajorHead
    {
        [Required]
        [MaxLength(4)]
        [Column("major_acc_code")]
        public string MajorAccCode { get; set; } = string.Empty;

    
        [Required]
        [MaxLength(2)]
        [Column("sub_major_acc_code")]
        public string SubMajorAccCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)] // DDD T-01..T-05 varchar(200) (BudgetP2AccountCodesAndAssignment)
        [Column("sub_major_acc_desc")]
        public string SubMajorAccDesc { get; set; } = string.Empty;

        [Required]
        [Column("modification_date")]
        public DateTime ModificationDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(6)]
        [Column("user_code")]
        public string UserCode { get; set; } = string.Empty;

        // Navigation property to parent Major Head
        [ForeignKey(nameof(MajorAccCode))]
        public virtual MajorHead? MajorHead { get; set; }
    }
}
