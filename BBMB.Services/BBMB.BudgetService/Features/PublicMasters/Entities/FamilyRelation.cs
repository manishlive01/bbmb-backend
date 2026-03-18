using BBMB.BudgetService.Features.Employees.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.PublicMasters.Entities
{

    [Table("family_relation_master")]
    public class FamilyRelation
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("family_relation_name")]
        public string FamilyRelationName { get; set; } = string.Empty;

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
        public ICollection<FamilyMember> FamilyMembers { get; set; } = new List<FamilyMember>();
    }
}
