namespace BBMB.BudgetService.Features.Estimates.Update.Dtos
{
    public class UpdateEstimateDto
    {
        public string? EstimateDesc { get; set; }
        public decimal? EstimateAmt { get; set; }
        public string? EstimateNarr { get; set; }
        public int FinYearEnd { get; set; }
        public int DivisionCode { get; set; }
    }
}
