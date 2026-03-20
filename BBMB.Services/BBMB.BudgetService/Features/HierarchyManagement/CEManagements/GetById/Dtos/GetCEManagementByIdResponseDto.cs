namespace BBMB.BudgetService.Features.HierarchyManagement.CEManagements.GetById.Dtos
{
    public class GetCEManagementByIdResponseDto
    {
        public int CEId { get; set; }
        public int CECode { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ProjectCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public DateTime? Date { get; set; }
        public bool Status { get; set; }
        public string UserCode { get; set; }
    }
}
