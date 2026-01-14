using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.Offices.Entities
{
    [Table("account_office")]
    public class AccountOffice
    {
        [Key]
        [MaxLength(2)]
        [Column("acc_office")]
        public string AccOffice { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("acc_office_desc")]
        public string AccOfficeDesc { get; set; } = string.Empty;

        [Required]
        [Column("modification_date")]
        public DateTime ModificationDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(6)]
        [Column("user_code")]
        public string UserCode { get; set; } = string.Empty;
    }
}
