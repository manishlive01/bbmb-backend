namespace BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Create.Dtos
{
    public class CreateCRSectionManagementDto
    {
        public int CRId { get; set; }
        public int CRSectionCode { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public bool Status { get; set; }
    }
}
