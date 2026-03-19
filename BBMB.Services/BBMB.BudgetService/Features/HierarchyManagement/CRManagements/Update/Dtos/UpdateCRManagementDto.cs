namespace BBMB.BudgetService.Features.HierarchyManagement.CRManagements.Update.Dtos
{
    public class UpdateCRManagementDto
    {
        public int CECode { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public bool Status { get; set; }
    }
}
