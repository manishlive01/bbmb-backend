namespace BBMB.BudgetService.Features.Budgets.BudgetTypes.GetList.Dtos
{
    public class GetBudgetTypeMasterListResponseDto
    {
        public int BudgetTypeMasterId { get; set; }
        public string BudgetTypeCode { get; set; }
        public string BudgetTypeName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
