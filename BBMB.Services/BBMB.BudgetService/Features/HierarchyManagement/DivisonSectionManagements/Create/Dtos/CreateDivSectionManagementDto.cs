namespace BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.Create.Dtos
{
    public class CreateDivSectionManagementDto
    {
        public int DivisionId { get; set; }
        public int DivSectionCode { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }

        public bool Status { get; set; }
    }
}
