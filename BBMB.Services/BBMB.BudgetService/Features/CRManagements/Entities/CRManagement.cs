using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Features.CEManagements.Entities;

namespace BBMB.BudgetService.Features.CRManagements.Entities
{
    [Table("cr_management")]
    public class CRManagement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("ce_id")]
        public int CEId { get; set; }

        [Required]
        [MaxLength(15)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("desc")]
        public string? Description { get; set; }

        [MaxLength(100)]
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

        [ForeignKey(nameof(CEId))]
        public virtual CEManagement? CEManagement { get; set; }
    }
}
