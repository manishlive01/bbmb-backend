using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB_Budget.Features.Geographical.States.Entities
{
    [Table("state_master")]
    public class StateMaster
    {
        [Key]
        [Column("state_master_id")]
        public int StateMasterId { get; set; }

        [Required]
        [MaxLength(5)]
        [Column("state_code")]
        public string StateCode { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("state_name")]
        public string StateName { get; set; }

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
    }
}
