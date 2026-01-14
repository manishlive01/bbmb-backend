using BBMB.BudgetService.Features.AccountingHeads.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.MinorHeads.Entities
{
    [Table("fa_minor_head")]
    public class MinorHead
    {
        [Key]
        [MaxLength(3)]
        [Column("minor_acc_code")]
        public string MinorAccCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("minor_acc_desc")]
        public string MinorAccDesc { get; set; } = string.Empty;

        [Required]
        [Column("modification_date")]
        public DateTime ModificationDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(6)]
        [Column("user_code")]
        public string UserCode { get; set; } = string.Empty;

        // Navigation property for related sub-minor heads
        public virtual ICollection<SubMinorHead> SubMinorHeads { get; set; } = new List<SubMinorHead>();
    }
}
