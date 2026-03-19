using BBMB.BudgetService.Features.HierarchyManagement.CEManagements.Entities;
using BBMB.BudgetService.Features.HierarchyManagement.CRManagements.Entities;
using BBMB.BudgetService.Features.HierarchyManagement.Divisions.Entities;
using BBMB.BudgetService.Features.HierarchyManagement.LevelManagement.Entites;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.Offices.OfficeManagements.Entities
{
    [Table("office_management")]
    public class OfficeManagement
    {
        [Key]
        [Column("office_code")]
        public string OfficeCode { get; set; } = null!;

        [Required]
        [Column("office_desc")]
        public string OfficeDesc { get; set; } = null!;

        [Required]
        [Column("ce_code")]
        public int CECode { get; set; }

        [Column("cr_code")]
        public int? CRCode { get; set; }

        [Column("division_code")]
        public int? DivisionCode { get; set; }
        [Column("level_id")]
        public int LevelId { get; set; }

        [Column("modification_date")]
        public DateTime? ModificationDate { get; set; }

        [Column("user_code")]
        public string? UserCode { get; set; }

        [ForeignKey(nameof(CECode))]
        public virtual CEManagement? CEManagement { get; set; }

        [ForeignKey(nameof(CRCode))]
        public virtual CRManagement? CRManagement { get; set; }

        [ForeignKey(nameof(DivisionCode))]
        public virtual Division? Divisions { get; set; }

        [ForeignKey(nameof(LevelId))]
        public virtual Levels Levels { get; set; }

    }
}
