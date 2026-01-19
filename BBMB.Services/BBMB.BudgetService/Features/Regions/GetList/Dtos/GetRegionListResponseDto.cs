namespace BBMB.BudgetService.Features.Regions.GetList.Dtos
{
    public class GetRegionListResponseDto
    {
        public string RegionCode { get; set; } = string.Empty;
        public string RegionDesc { get; set; } = string.Empty;
        public DateTime ModificationDate { get; set; }
        public string UserCode { get; set; } = string.Empty;
    }
}
