using BBMB.BudgetService.Features.HierarchyManagement.Divisions.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.Entities
{
    [Table("div_section")]
    public class DivSection
    {
        [Column("id")]
        public int Id { get; set; }  // ✅ PK
        [Column("division_id")]
        public int DivisionId { get; set; } // ✅ FK

        [Required]
        public int DivisionCode { get; set; }   // FK

        [Required]
        [Column("ce_code")]
        public int CECode { get; set; }

        [Required]
        [Column("cr_code")]
        public int CRCode { get; set; }

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
        public virtual Division Division { get; set; } = null!;
    }
}
