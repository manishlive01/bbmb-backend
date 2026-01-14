using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.DetailHeads.Entities
{
    [Table("fa_detail_head")]
    public class DetailHead
    {
        [Key]
        [MaxLength(3)]
        [Column("detail_acc_code")]
        public string DetailAccCode { get; set; } = string.Empty;

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
    }
}
