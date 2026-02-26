using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.ObjectClassifications.Entities
{
    [Table("fa_object_head")]
    public class ObjectClassification
    {
        [Key]
        [MaxLength(3)]
        [Column("object_acc_code")]
        public string ObjectAccCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        [Column("object_acc_desc")]
        public string ObjectAccDesc { get; set; } = string.Empty;

        [Required]
        [Column("modification_date")]
        public DateTime ModificationDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(6)]
        [Column("user_code")]
        public string UserCode { get; set; } = string.Empty;
    }
}
