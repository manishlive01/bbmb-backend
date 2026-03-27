using BBMB.BudgetService.Features.HierarchyManagement.CRManagements.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Entites
{
    [Table("cr_section")]
    public class CRSection
    {
        [Column("id")]
        public int Id { get; set; }// ✅ PK 

        [Column("cr_id")]
        public int CRId { get; set; } // ✅ FK
        [Column("cr_section_code")]
        public int CRSectionCode { get; set; }
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
        // Navigation
        public virtual CRManagement CRManagement { get; set; } = null!;
    }
}
