namespace BBMB_Budget.Features.Geographical.States.GetById.Dtos
{
    public class GetStateMasterByIdResponseDto
    {
        public string StateCode { get; set; }
        public string StateName { get; set; }
        public bool IsActive { get; set; }
    }
}
