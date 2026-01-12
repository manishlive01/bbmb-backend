namespace BBMB.BudgetService.Features.Geographical.Districts.GetById.Dtos
{
    public class GetDistrictMasterByIdResponseDto
    {
        public int StateMasterId { get; set; }
        public string StateName { get; set; }
        public string DistrictCode { get; set; }
        public string DistrictName { get; set; }
        public bool IsActive { get; set; }
    }
}
