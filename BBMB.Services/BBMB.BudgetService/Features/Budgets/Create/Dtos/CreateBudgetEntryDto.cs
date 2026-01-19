using BBMB.BudgetService.Features.Budgets.Entities;

namespace BBMB.BudgetService.Features.Budgets.Create.Dtos
{
    public class CreateBudgetEntryDto
    {
        public int DivisionCode { get; set; }
        public int DivSection { get; set; }
        public int CalendarYear { get; set; }
        public int SequenceNo { get; set; }
        public string MajorAccCode { get; set; } = string.Empty;
        public string SubMajorAccCode { get; set; } = string.Empty;
        public string MinorAccCode { get; set; } = string.Empty;
        public string SubMinorAccCode { get; set; } = string.Empty;
        public string DetailAccCode { get; set; } = string.Empty;
        public string ObjectAccCode { get; set; } = string.Empty;
        public string? WorkCode { get; set; }
        public string BudgetDescription { get; set; } = string.Empty;
        public decimal? BudgetAmount { get; set; }
        public decimal? RevisedAmount { get; set; }
        public decimal? ProposedBudget { get; set; }
        public BudgetType BudgetType { get; set; }
        public string UserCode { get; set; } = string.Empty;
    }
}
