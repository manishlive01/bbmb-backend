using BBMB.BudgetService.Features.Employees.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.EmployeeCharges.Entities
{
    [Table("employee_charge")]
    public class EmployeeCharge
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("employee_id")]
        public Guid EmployeeId { get; set; }

        [Column("project_code")]
        public string ProjectCode { get; set; }

        [Required]
        [Column("ce_id")]
        public int? CEId { get; set; }

        [Column("cr_id")]
        public int? CRId { get; set; }

        [Column("division_id")]
        public int? DivisionId { get; set; }

        [Column("charge_time")]
        public DateTime ChargeTime { get; set; }

        [Column("charge_expire_time")]
        public DateTime? ChargeExpireTime { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("is_charge_expire")]
        public bool IsChargeExpire { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; } = null!;
    }
}
