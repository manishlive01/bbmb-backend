using BBMB.BudgetService.Features.Employees.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.PublicMasters.Titles.Entities
{
    [Table("title_master")]
    public class Title
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("title_name")]
        public string TitleName { get; set; } = string.Empty;

        [Column("is_status")]
        public bool IsStatus { get; set; } = true;

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }
        public ICollection<EmploymentDetail> EmploymentDetails { get; set; } = new List<EmploymentDetail>();
    }
}