namespace BBMB.BudgetService.Features.Geographical.States.Update.Dtos
{
    public class UpdateStateMasterDto
    {
        public int StateMasterId { get; set; }
        public string StateCode { get; set; }
        public string StateName { get; set; }
        public bool IsActive { get; set; }
    }
}
