namespace BBMB.BudgetService.Features.Designations.Create.Dtos
{
    public class CreateDesignationDto
    {
        public string DesignationCode { get; set; } = string.Empty;
        public string DesignationDesc { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
    }
}
