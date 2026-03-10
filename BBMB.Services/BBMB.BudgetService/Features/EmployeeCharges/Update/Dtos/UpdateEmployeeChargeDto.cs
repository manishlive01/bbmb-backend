namespace BBMB.BudgetService.Features.EmployeeCharges.Update.Dtos
{
    public class UpdateEmployeeChargeDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public int ProjectId { get; set; }

        public string CECode { get; set; } = null!;
        public string CRCode { get; set; } = null!;
        public string DivCode { get; set; } = null!;

        public DateTime ChargeTime { get; set; }
        public DateTime? ChargeExpireTime { get; set; }

        public bool IsActive { get; set; }
        public bool IsChargeExpire { get; set; }
    }
}
