using BBMB.BudgetService.Features.Classes.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.Designations.Entities
{
    [Table("designation")]
    public class Designation
    {
        [Key]
        [MaxLength(4)]
        [Column("designation_code")]
        public string DesignationCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("designation_desc")]
        public string DesignationDesc { get; set; } = string.Empty;

        [Required]
        [MaxLength(1)]
        [Column("class_code")]
        public string ClassCode { get; set; } = string.Empty;

        [Column("min_basic")]
        public decimal? MinBasic { get; set; }

        [Column("max_basic")]
        public decimal? MaxBasic { get; set; }

        [Required]
        [Column("modification_date")]
        public DateTime ModificationDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(6)]
        [Column("user_code")]
        public string UserCode { get; set; } = string.Empty;

        // Navigation property
        [ForeignKey("ClassCode")]
        public virtual Class Class { get; set; } = null!;
    }
}
