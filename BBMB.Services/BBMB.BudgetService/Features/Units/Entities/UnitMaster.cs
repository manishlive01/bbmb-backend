using BBMB.BudgetService.Features.PowerIrrigations.Entities;
using BBMB.BudgetService.Features.Projects.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.Units.Entities
{
    [Table("units")]
    public class UnitMaster
    {
        [Key]
        [MaxLength(2)]
        [Column("unit_code")]
        public string UnitCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("unit_desc")]
        public string UnitDesc { get; set; } = string.Empty;

        [Required]
        [MaxLength(1)]
        [Column("proj_code")]
        public string ProjCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(2)]
        [Column("irr_pwr_code")]
        public string IrrPwrCode { get; set; } = string.Empty;

        [Required]
        [Column("modification_date")]
        public DateTime ModificationDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(6)]
        [Column("user_code")]
        public string UserCode { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("ProjCode")]
        public virtual Project Project { get; set; } = null!;

        [ForeignKey("IrrPwrCode")]
        public virtual PowerIrrigation PowerIrrigation { get; set; } = null!;
    }
}
