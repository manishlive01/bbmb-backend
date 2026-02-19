using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.CEManagements.Entities
{
    [Table("ce_management")]
    public class CEManagement
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(15)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(2)]
        [Column("project_code")]
        public string ProjectCode { get; set; } = string.Empty;

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
    }
}
