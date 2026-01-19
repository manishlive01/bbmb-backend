using BBMB.BudgetService.Features.Budgets.Entities;

namespace BBMB.BudgetService.Features.Budgets.GetById.Dtos
{
    public class GetBudgetEntryByIdResponseDto
    {
        public int Id { get; set; }
        public int DivisionCode { get; set; }
        public string? DivisionDesc { get; set; }
        public int DivSection { get; set; }
        public int CalendarYear { get; set; }
        public int SequenceNo { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string? AccountDescription { get; set; }
        public string? WorkCode { get; set; }
        public string? WorkDesc { get; set; }
        public string BudgetDescription { get; set; } = string.Empty;
        public decimal? BudgetAmount { get; set; }
        public decimal? RevisedAmount { get; set; }
        public decimal? ProposedBudget { get; set; }
        public BudgetType BudgetType { get; set; }
        public string UserCode { get; set; } = string.Empty;
    }
}
