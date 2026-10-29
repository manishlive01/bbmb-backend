namespace BBMB.BudgetService.Features.EmployeeCharges.GetList.Dtos
{
    public class GetEmployeeChargeResponseDto
    {
        public Guid Id { get; set; }

        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string? ProfileImageUrl { get; set; }

        public string ProjectCode { get; set; }

        public int? CEId { get; set; }
        public int? CECode { get; set; }
        public string? CEName { get; set; } = null!;
        public int? CRId { get; set; }
        public int? CRCode { get; set; }
        public string? CRName { get; set; } = null!;
        public int? DivisionId { get; set; }
        public int? DivisionCode { get; set; }
        public string? DivName { get; set; } = null!;
        public DateTime ChargeTime { get; set; }

        public DateTime? ChargeExpireTime { get; set; }

        public bool IsActive { get; set; }

        public bool IsChargeExpire { get; set; }
    }
}