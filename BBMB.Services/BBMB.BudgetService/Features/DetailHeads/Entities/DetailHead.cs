using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.DetailHeads.Entities
{
    [Table("fa_detail_head")]
    public class DetailHead
    {
        [Key]
        [MaxLength(2)]
        [Column("detail_acc_code")]
        public string DetailAccCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(4)]
        [Column("major_acc_code")]
        public string MajorAccCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(2)]
        [Column("sub_major_acc_code")]
        public string SubMajorAccCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(3)]
        [Column("minor_acc_code")]
        public string MinorAccCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(2)]
        [Column("sub_minor_acc_code")]
        public string SubMinorAccCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("detail_acc_desc")]
        public string DetailAccDesc { get; set; } = string.Empty;

        [Required]
        [Column("modification_date")]
        public DateTime ModificationDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(6)]
        [Column("user_code")]
        public string UserCode { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey(nameof(MajorAccCode))]
        public virtual MajorHead? MajorHead { get; set; }

        [ForeignKey(nameof(SubMajorAccCode))]
        public virtual SubMajorHead? SubMajorHead { get; set; }

        [ForeignKey(nameof(MinorAccCode))]
        public virtual MinorHead? MinorHead { get; set; }

        [ForeignKey(nameof(SubMinorAccCode))]
        public virtual SubMinorHead? SubMinorHead { get; set; }
    }
}
