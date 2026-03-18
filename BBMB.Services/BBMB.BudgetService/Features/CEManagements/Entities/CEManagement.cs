using BBMB.BudgetService.Features.CRManagements.Entities;
using BBMB.BudgetService.Features.Divisions.Entities;
using BBMB.BudgetService.Features.Offices.OfficeManagements.Entities;
using BBMB.BudgetService.Features.Projects.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.CEManagements.Entities
{
    [Table("ce_management")]
    public class CEManagement
    {
        [Key]
        [Required]
        [Column("ce_code")]
        public int CECode { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(2)]
        [Column("project_code")]
        public string ProjectCode { get; set; } = string.Empty;

        [MaxLength(200)]
        [Column("desc")]
        public string? Description { get; set; }

        [MaxLength(200)]
        [Column("location")]
        public string? Location { get; set; }

        [Column("date")]
        public DateTime? Date { get; set; }

        [Required]
        [Column("status")]
        public bool Status { get; set; }

        [Required]
        [MaxLength(6)]
        [Column("user_code")]
        public string UserCode { get; set; } = string.Empty;


        [ForeignKey(nameof(ProjectCode))]
        public virtual Project? Projects { get; set; }

        public virtual ICollection<CRManagement> CRManagements { get; set; } = new List<CRManagement>();
        public virtual ICollection<Division> Divisions { get; set; } = new List<Division>();
        public virtual ICollection<OfficeManagement> OfficeManagements { get; set; } = new List<OfficeManagement>();
    }
}
