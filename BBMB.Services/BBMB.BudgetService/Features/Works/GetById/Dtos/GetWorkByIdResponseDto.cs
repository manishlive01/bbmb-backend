namespace BBMB.BudgetService.Features.Works.GetById.Dtos
{
    public class GetWorkByIdResponseDto
    {
        // P4 (design §4.6): work_code is varchar(12) and nullable (null until generated on allocation, BUD-232).
        public string? WorkCode { get; set; }
        public string WorkDesc { get; set; } = string.Empty;
        public int DivisionCode { get; set; }
        public string? DivisionDesc { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string? AccountDescription { get; set; }
        public string UserCode { get; set; } = string.Empty;
        public string? FinYrStart { get; set; }
        public string? FinYrEnd { get; set; }
        public string? SanctionNo { get; set; }
        public string? SanctionDate { get; set; }
        public string? DesignationCode { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? Status { get; set; }
        public string? Nature { get; set; }
        public string? RefWorkCode { get; set; }
    }
}
