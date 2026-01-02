namespace BBMB_Budget.Features.Geographical.Districts.Create.Dtos
{
    public class CreateDistrictMasterDto
    {
        public int StateMasterId { get; set; }
        public string DistrictCode { get; set; }
        public string DistrictName { get; set; }
        public bool IsActive { get; set; }

    }
}
