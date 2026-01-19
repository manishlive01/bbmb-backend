namespace BBMB.BudgetService.Features.Divisions.GetList.Dtos
{
    public class GetDivisionListResponseDto
    {
        public int DivisionCode { get; set; }
        public int DivSection { get; set; }
        public string DivisionDesc { get; set; } = string.Empty;
        public string? RegionCode { get; set; }
        public string? PoolCode { get; set; }
        public DateTime ModificationDate { get; set; }
        public string UserCode { get; set; } = string.Empty;
    }
}
