using BBMB.BudgetService.Features.Geographical.States.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.Geographical.Districts.Entities
{
    [Table("district_master")]
    public class DistrictMaster
    {
        [Key]
        [Column("district_master_id")]
        public int DistrictMasterId { get; set; }

        [Column("state_master_id")]
        public int StateMasterId { get; set; }

        [Required]
        [MaxLength(5)]
        [Column("district_code")]
        public string DistrictCode { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("district_name")]
        public string DistrictName { get; set; }

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

        [ForeignKey(nameof(StateMasterId))]
        public virtual StateMaster StateMasters { get; set; }

    }
}
