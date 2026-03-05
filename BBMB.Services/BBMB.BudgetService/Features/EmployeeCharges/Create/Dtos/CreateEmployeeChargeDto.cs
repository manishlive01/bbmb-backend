namespace BBMB.BudgetService.Features.EmployeeCharges.Create.Dtos
{
    public class CreateEmployeeChargeDto
    {
        public Guid EmployeeId { get; set; }

        public Guid UserId { get; set; }

        public int ProjectId { get; set; }

        public string CECode { get; set; } = null!;
        public string CRCode { get; set; } = null!;
        public string DivCode { get; set; } = null!;

        public DateTime ChargeTime { get; set; }

        public DateTime? ChargeExpireTime { get; set; }
    }
}
