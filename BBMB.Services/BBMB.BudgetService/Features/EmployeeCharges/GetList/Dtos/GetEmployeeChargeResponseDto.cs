namespace BBMB.BudgetService.Features.EmployeeCharges.GetByUser.Dtos
{
    public class GetEmployeeChargeResponseDto
    {
        public Guid Id { get; set; }

        public Guid EmployeeId { get; set; }

        public string ProjectCode { get; set; }

        public string CECode { get; set; } = string.Empty;

        public string CRCode { get; set; } = string.Empty;

        public int DivisionCode { get; set; }

        public DateTime ChargeTime { get; set; }

        public DateTime? ChargeExpireTime { get; set; }

        public bool IsActive { get; set; }

        public bool IsChargeExpire { get; set; }
    }
}