namespace BBMB.BudgetService.Shared.ServiceDTOs
{
    public class UserChargeContext
    {
        public Guid UserId { get; set; }
        public Guid EmployeeId { get; set; }

        /// <summary>employee.employee_charge id of the selected charge (design §4.3, D-12). Additive.</summary>
        public Guid ChargeId { get; set; }
        public string ProjectCode { get; set; }
        public int? CEId { get; set; }
        public int? CRId { get; set; }
        public int? DivisionId { get; set; }
    }
}
