using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.Classes.Entities
{
    [Table("class")]
    public class Class
    {
        [Key]
        [MaxLength(1)]
        [Column("class_code")]
        public string ClassCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("class_desc")]
        public string ClassDesc { get; set; } = string.Empty;

        [Required]
        [Column("modification_date")]
        public DateTime ModificationDate { get; set; } = DateTime.UtcNow;
    }
}
