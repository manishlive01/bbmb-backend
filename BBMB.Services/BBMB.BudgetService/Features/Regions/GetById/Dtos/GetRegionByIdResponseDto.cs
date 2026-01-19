namespace BBMB.BudgetService.Features.Regions.GetById.Dtos
{
    public class GetRegionByIdResponseDto
    {
        public string RegionCode { get; set; } = string.Empty;
        public string RegionDesc { get; set; } = string.Empty;
        public DateTime ModificationDate { get; set; }
        public string UserCode { get; set; } = string.Empty;
    }
}
