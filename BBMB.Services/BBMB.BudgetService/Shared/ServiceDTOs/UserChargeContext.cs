namespace BBMB.BudgetService.Shared.ServiceDTOs
{
    public class UserChargeContext
    {
        public Guid UserId { get; set; }
        public Guid EmployeeId { get; set; }

        public string ProjectCode { get; set; }

        public int CECode { get; set; }
        public int CRCode { get; set; }
        public int DivisionCode { get; set; }
    }
}
