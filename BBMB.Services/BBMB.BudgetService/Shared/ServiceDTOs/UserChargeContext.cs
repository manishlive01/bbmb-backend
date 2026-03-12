namespace BBMB.BudgetService.Shared.ServiceDTOs
{
    public class UserChargeContext
    {
        public Guid UserId { get; set; }
        public Guid EmployeeId { get; set; }

        public string ProjectCode { get; set; }

        public string CECode { get; set; } = null!;
        public string CRCode { get; set; } = null!;
        public int DivisionCode { get; set; }
    }
}
