namespace BBMB.BudgetService.Features.EmployeeCharges.Update.Dtos
{
    public class UpdateEmployeeChargeDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string ProjectCode { get; set; }

        public int? CEId { get; set; }
        public int? CRId { get; set; }
        public int? DivisionId { get; set; }

        public DateTime ChargeTime { get; set; }
        public DateTime? ChargeExpireTime { get; set; }

        public bool IsActive { get; set; }
    }
}
