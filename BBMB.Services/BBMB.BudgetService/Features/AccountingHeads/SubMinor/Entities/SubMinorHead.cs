using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Entities;
using BBMB.BudgetService.Features.DetailHeads.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinor.Entities
{
    [Table("fa_sub_minor_head")]
    public class SubMinorHead
    { 
        [MaxLength(2)]
        [Column("sub_minor_acc_code")]
        public string SubMinorAccCode { get; set; } = string.Empty;
         
        [Required]
        [MaxLength(3)]
        [Column("minor_acc_code")]
        public string MinorAccCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)] // DDD T-01..T-05 varchar(200) (BudgetP2AccountCodesAndAssignment)
        [Column("sub_minor_acc_desc")]
        public string SubMinorAccDesc { get; set; } = string.Empty;

        [Required]
        [Column("modification_date")]
        public DateTime ModificationDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(6)]
        [Column("user_code")]
        public string UserCode { get; set; } = string.Empty;


        [ForeignKey(nameof(MinorAccCode))]
        public virtual MinorHead? MinorHead { get; set; }

        public virtual ICollection<DetailHead> DetailHeads { get; set; } = new List<DetailHead>();
    }
}
