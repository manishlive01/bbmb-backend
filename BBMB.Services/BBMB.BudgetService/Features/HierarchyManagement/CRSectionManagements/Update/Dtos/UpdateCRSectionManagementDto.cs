namespace BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Update.Dtos
{
    public class UpdateCRSectionManagementDto
    {
        public int Id { get; set; }
        public int CRId { get; set; }
        public int CRSectionCode { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }

        public bool Status { get; set; }
    }
}