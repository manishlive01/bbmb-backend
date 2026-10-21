namespace BBMB.BudgetService.Features.Estimates.GetById.Dtos
{
    public class GetEstimateByIdResponseDto
    {
        // P4 (design §4.6): the estimate is keyed on work_id; work_code is the retained pre-reset int (OI-46).
        public int WorkId { get; set; }
        public int WorkCode { get; set; }
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
