namespace BBMB.BudgetService.Features.Estimates.GetById.Dtos
{
    public class GetEstimateByIdResponseDto
    {
        public string WorkCode { get; set; } = string.Empty;
        public string EstimateCode { get; set; } = string.Empty;
        public string? EstimateDesc { get; set; }
        public decimal? EstimateAmt { get; set; }
        public string? EstimateNarr { get; set; }
        public int FinYearEnd { get; set; }
        public int DivisionCode { get; set; }
        
        // Joined data
        public string? WorkDesc { get; set; }
        public string? DivisionDesc { get; set; }
    }
}
