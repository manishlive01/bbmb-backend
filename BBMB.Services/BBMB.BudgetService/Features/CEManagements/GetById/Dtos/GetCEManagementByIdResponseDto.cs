namespace BBMB.BudgetService.Features.CEManagements.GetById.Dtos
{
    public class GetCEManagementByIdResponseDto
    {
        public string CECode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ProjectCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public DateTime? Date { get; set; }
        public bool Status { get; set; }
        public string UserCode { get; set; }
    }
}
