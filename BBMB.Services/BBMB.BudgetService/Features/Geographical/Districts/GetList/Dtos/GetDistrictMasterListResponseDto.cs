namespace BBMB.BudgetService.Features.Geographical.Districts.GetList.Dtos
{
    public class GetDistrictMasterListResponseDto
    {
        public int DistrictMasterId { get; set; }
        public int StateMasterId { get; set; }
        public string StateName { get; set; }
        public string DistrictCode { get; set; }
        public string DistrictName { get; set; }
        public bool IsActive { get; set; }
    }
}
