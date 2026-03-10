namespace BBMB.BudgetService.Features.Divisions.GetByCECR.Dtos
{
    public class GetDivisionByCECRResponseDto
    {
        public int DivisionCode { get; set; }
        public int DivSection { get; set; }
        public string DivisionDesc { get; set; } = string.Empty;
        public string? RegionCode { get; set; }
        public string? PoolCode { get; set; }
        public DateTime ModificationDate { get; set; }
        public string CECode { get; set; } = string.Empty;
        public string CRCode { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
    }
}
