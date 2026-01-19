namespace BBMB.BudgetService.Features.Projects.Create.Dtos
{
    public class CreateProjectDto
    {
        public string ProjCode { get; set; } = string.Empty;
        public string ProjDesc { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
    }
}
