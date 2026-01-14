using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Entities
{
    [Table("fa_major_head")]
    public class MajorHead
    {
        [Key]
        [MaxLength(4)]
        [Column("major_acc_code")]
        public string MajorAccCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("major_acc_desc")]
        public string MajorAccDesc { get; set; } = string.Empty;

        [Required]
        [Column("modification_date")]
        public DateTime ModificationDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(6)]
        [Column("user_code")]
        public string UserCode { get; set; } = string.Empty;

        // Navigation property for related sub-major heads
        public virtual ICollection<SubMajorHead> SubMajorHeads { get; set; } = new List<SubMajorHead>();
    }
}
