namespace BBMB.BudgetService.Shared.ServiceDTOs
{
    public class UserChargeContext
    {
        public Guid UserId { get; set; }
        public Guid EmployeeId { get; set; }

        public string ProjectCode { get; set; }

        public int? CEId { get; set; }
        public int? CRId { get; set; }
        public int? DivisionId { get; set; }
    }
}
