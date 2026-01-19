namespace BBMB.BudgetService.Features.Works.GetList.Dtos
{
    public class GetWorkListResponseDto
    {
        public string WorkCode { get; set; } = string.Empty;
        public string WorkDesc { get; set; } = string.Empty;
        public int DivisionCode { get; set; }
        public string? DivisionDesc { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string? AccountDescription { get; set; }
        public string? Status { get; set; }
        public string? Nature { get; set; }
        public string UserCode { get; set; } = string.Empty;
    }
}
