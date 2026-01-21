using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Entities;
using BBMB.BudgetService.Features.DetailHeads.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Entities
{
    [Table("fa_sub_minor_head")]
    public class SubMinorHead
    {
        [Key]
        [MaxLength(2)]
        [Column("sub_minor_acc_code")]
        public string SubMinorAccCode { get; set; } = string.Empty;

        //[Required]
        //[MaxLength(4)]
        //[Column("major_acc_code")]
        //public string MajorAccCode { get; set; } = string.Empty;

        //[Required]
        //[MaxLength(2)]
        //[Column("sub_major_acc_code")]
        //public string SubMajorAccCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(3)]
        [Column("minor_acc_code")]
        public string MinorAccCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("sub_minor_acc_desc")]
        public string SubMinorAccDesc { get; set; } = string.Empty;

        [Required]
        [Column("modification_date")]
        public DateTime ModificationDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(6)]
        [Column("user_code")]
        public string UserCode { get; set; } = string.Empty;

        //// Navigation properties
        //[ForeignKey(nameof(MajorAccCode))]
        //public virtual MajorHead? MajorHead { get; set; }

        //[ForeignKey(nameof(SubMajorAccCode))]
        //public virtual SubMajorHead? SubMajorHead { get; set; }

        [ForeignKey(nameof(MinorAccCode))]
        public virtual MinorHead? MinorHead { get; set; }

        public virtual ICollection<DetailHead> DetailHeads { get; set; } = new List<DetailHead>();
    }
}
