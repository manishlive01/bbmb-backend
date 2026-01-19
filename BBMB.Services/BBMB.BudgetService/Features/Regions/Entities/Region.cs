using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.Regions.Entities
{
    [Table("region")]
    public class Region
    {
        [Key]
        [MaxLength(2)]
        [Column("region_code")]
        public string RegionCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("region_desc")]
        public string RegionDesc { get; set; } = string.Empty;

        [Required]
        [Column("modification_date")]
        public DateTime ModificationDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(6)]
        [Column("user_code")]
        public string UserCode { get; set; } = string.Empty;
    }
}
