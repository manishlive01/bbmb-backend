using BBMB.BudgetService.Features.CEManagements.Entities;
using BBMB.BudgetService.Features.DetailHeads.Entities;
using BBMB.BudgetService.Features.Units.Entities;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.Projects.Entities
{
    [Table("projects")]
    public class Project
    {
        [Key]
        [MaxLength(1)]
        [Column("proj_code")]
        public string ProjCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("proj_desc")]
        public string ProjDesc { get; set; } = string.Empty;

        [Required]
        [Column("modification_date")]
        public DateTime ModificationDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(6)]
        [Column("user_code")]
        public string UserCode { get; set; } = string.Empty;

        // Navigation property for related units
        public virtual ICollection<UnitMaster> UnitMasters { get; set; } = new List<UnitMaster>();
        public virtual ICollection<CEManagement> CEManagements { get; set; } = new List<CEManagement>();
    }
}
