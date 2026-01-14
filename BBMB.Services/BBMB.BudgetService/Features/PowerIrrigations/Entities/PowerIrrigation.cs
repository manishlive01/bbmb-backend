using BBMB.BudgetService.Features.Units.Entities;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.PowerIrrigations.Entities
{
    [Table("irr_pwr")]
    public class PowerIrrigation
    {
        [Key]
        [MaxLength(2)]
        [Column("irr_pwr_code")]
        public string IrrPwrCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("irr_pwr_desc")]
        public string IrrPwrDesc { get; set; } = string.Empty;

        [Required]
        [Column("modification_date")]
        public DateTime ModificationDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(6)]
        [Column("user_code")]
        public string UserCode { get; set; } = string.Empty;

        // Navigation property for related units
        public virtual ICollection<UnitMaster> Units { get; set; } = new List<UnitMaster>();
    }
}
