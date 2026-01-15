namespace BBMB.BudgetService.Features.Regions.Create.Dtos
{
    public class CreateRegionDto
    {
        public string RegionCode { get; set; } = string.Empty;
        public string RegionDesc { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
    }
}
