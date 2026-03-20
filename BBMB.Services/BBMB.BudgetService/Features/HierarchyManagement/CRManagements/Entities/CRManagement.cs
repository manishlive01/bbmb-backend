using BBMB.BudgetService.Features.HierarchyManagement.CEManagements.Entities;
using BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Entites;
using BBMB.BudgetService.Features.HierarchyManagement.Divisions.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRManagements.Entities
{
    [Table("cr_management")]
    public class CRManagement
    {
        [Column("id")]
        public int Id { get; set; } 

        [Required]
        [Column("cr_code")]
        public int CRCode { get; set; } 

        [Required]
        [Column("ce_id")]

        public int CEId { get; set; } // ✅ FK
        [Required]
        [MaxLength(200)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

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
        public virtual CEManagement? CEManagement { get; set; }
        public virtual ICollection<Division> Divisions { get; set; } = new List<Division>();
        public virtual ICollection<CRSection> CRSections { get; set; } = new List<CRSection>();
    }
}
