namespace BBMB.BudgetService.Features.EmployeeCharges.Update.Dtos
{
    public class UpdateEmployeeChargeDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string ProjectCode { get; set; }

        public string CECode { get; set; } = null!;
        public string CRCode { get; set; } = null!;
        public int DivisionCode { get; set; }

        public DateTime ChargeTime { get; set; }
        public DateTime? ChargeExpireTime { get; set; }

        public bool IsActive { get; set; }
        public bool IsChargeExpire { get; set; }
    }
}
