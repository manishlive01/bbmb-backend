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
        // P4 (design §4.6): the budget entry references the work by its stable id, not the old int work code
        // (every work code is null after the reset). Optional: a budget entry need not name a work.
        public int? WorkId { get; set; }
        public string BudgetDescription { get; set; } = string.Empty;
        public decimal? BudgetAmount { get; set; }
        public decimal? RevisedAmount { get; set; }
        public decimal? ProposedBudget { get; set; }
        public BudgetType BudgetType { get; set; }
        public string UserCode { get; set; } = "123";
    }
}
