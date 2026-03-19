namespace BBMB.BudgetService.Features.HierarchyManagement.Divisions.GetByCECR.Dtos
{
    public class GetDivisionByCECRResponseDto
    {
        public int DivisionCode { get; set; }
        public int DivSection { get; set; }
        public string DivisionDesc { get; set; } = string.Empty;
        public string? RegionCode { get; set; }
        public string? PoolCode { get; set; }
        public DateTime ModificationDate { get; set; }
        public int CECode { get; set; } 
        public int CRCode { get; set; } 
        public string UserCode { get; set; } = string.Empty;
    }
}
