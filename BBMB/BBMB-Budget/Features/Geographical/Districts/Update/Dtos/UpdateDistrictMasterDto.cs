namespace BBMB_Budget.Features.Geographical.Districts.Update.Dtos
{
    public class UpdateDistrictMasterDto
    {
        public int DistrictMasterId { get; set; }
        public int StateMasterId { get; set; }
        public string DistrictCode { get; set; }
        public string DistrictName { get; set; }
        public bool IsActive { get; set; }
    }
}
