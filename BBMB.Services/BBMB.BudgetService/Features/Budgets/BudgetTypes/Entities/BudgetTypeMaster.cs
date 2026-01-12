using BBMB.BudgetService.Features.Budgets.Budgets.Entities; 
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.Budgets.BudgetTypes.Entities
{
    [Table("budget_type_master")]
    public class BudgetTypeMaster
    {
        [Key]
        [Column("budget_type_master_id")]
        public int BudgetTypeMasterId { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("budget_type_code")]
        public string BudgetTypeCode { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("budget_type_name")]
        public string BudgetTypeName { get; set; }

        [MaxLength(250)]
        [Column("description")]
        public string? Description { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }
        public virtual ICollection<BudgetMaster> BudgetMasters { get; set; } = new List<BudgetMaster>();
    }
}
