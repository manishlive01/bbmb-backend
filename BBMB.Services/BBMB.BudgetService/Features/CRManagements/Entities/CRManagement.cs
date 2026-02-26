using BBMB.BudgetService.Features.CEManagements.Entities;
using BBMB.BudgetService.Features.Divisions.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.CRManagements.Entities
{
    [Table("cr_management")]
    public class CRManagement
    {
        [Key]
        [Required]
        [Column("cr_code")]
        public string CRCode { get; set; } = string.Empty;


        [Required]
        [Column("ce_code")]
        public string CECode { get; set; } = string.Empty;

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

        [ForeignKey(nameof(CECode))]
        public virtual CEManagement? CEManagement { get; set; }
        public virtual ICollection<Division> Divisions { get; set; } = new List<Division>();
    }
}
