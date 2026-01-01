namespace BBMB_Budget.Features.Geographical.States.GetList.Dtos
{
    public class GetStateMasterListResponseDto
    {
        public int StateMasterId { get; set; }
        public string StateCode { get; set; }
        public string StateName { get; set; }
        public bool IsActive { get; set; }
    }
}
