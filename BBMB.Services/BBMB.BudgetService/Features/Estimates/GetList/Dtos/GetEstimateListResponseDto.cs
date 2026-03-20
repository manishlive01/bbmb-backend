namespace BBMB.BudgetService.Features.Estimates.GetList.Dtos
{
    public class GetEstimateListResponseDto
    {
        public int WorkCode { get; set; }  
        public string EstimateCode { get; set; } = string.Empty;
        public string? EstimateDesc { get; set; }
        public decimal? EstimateAmt { get; set; }
        public int FinYearEnd { get; set; }
        public int DivisionCode { get; set; }
        public string? WorkDescription { get; set; }
        public string? DivisionDescription { get; set; }
    }
}
