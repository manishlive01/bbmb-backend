namespace BBMB.BudgetService.Features.Budgets.BudgetTypes.Update.Dtos
{
    public class UpdateBudgetTypeMasterDto
    {
        public int BudgetTypeMasterId { get; set; }
        public string BudgetTypeCode { get; set; }
        public string BudgetTypeName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
