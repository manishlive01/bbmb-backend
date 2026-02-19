namespace BBMB.BudgetService.Features.CEManagements.Create.Dtos
{
    public class CreateCEManagementDto
    {
        public string Name { get; set; } = string.Empty;
        public string ProjectCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public DateTime? Date { get; set; }
        public bool Status { get; set; }
        public string UserCode { get; set; } = "123";
    }
}
