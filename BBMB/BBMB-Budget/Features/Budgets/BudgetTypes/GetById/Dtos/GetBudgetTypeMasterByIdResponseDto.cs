namespace BBMB_Budget.Features.Budgets.BudgetTypes.GetById.Dtos
{
    public class GetBudgetTypeMasterByIdResponseDto
    {
        public string BudgetTypeCode { get; set; }
        public string BudgetTypeName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
