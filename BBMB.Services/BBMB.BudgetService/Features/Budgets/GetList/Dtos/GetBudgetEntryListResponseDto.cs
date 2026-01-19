using BBMB.BudgetService.Features.Budgets.Entities;

namespace BBMB.BudgetService.Features.Budgets.GetList.Dtos
{
    public class GetBudgetEntryListResponseDto
    {
        public int Id { get; set; }
        public int DivisionCode { get; set; }
        public string? DivisionDesc { get; set; }
        public int CalendarYear { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string? AccountDescription { get; set; }
        public string? WorkCode { get; set; }
        public string? WorkDesc { get; set; }
        public string BudgetDescription { get; set; } = string.Empty;
        public decimal? BudgetAmount { get; set; }
        public BudgetType BudgetType { get; set; }
        public string UserCode { get; set; } = string.Empty;
    }
}
