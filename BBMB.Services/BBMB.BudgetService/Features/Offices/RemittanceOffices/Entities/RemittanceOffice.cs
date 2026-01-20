using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.Offices.RemittanceOffices.Entities
{
    [Table("remit_office")]
    public class RemittanceOffice
    {
        [Key]
        [MaxLength(3)]
        [Column("ro_code")]
        public string RoCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("ro_desc")]
        public string RoDesc { get; set; } = string.Empty;

        [Required]
        [Column("modification_date")]
        public DateTime ModificationDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(6)]
        [Column("user_code")]
        public string UserCode { get; set; } = string.Empty;
    }
}
