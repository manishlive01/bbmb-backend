namespace BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.GetList.Dtos
{
    public class GetDivSectionManagementListResponseDto
    {
        public int Id { get; set; }
        public int DivisionId { get; set; }

        public int DivisionCode { get; set; }   // ✅ NEW
        public string DivisionName { get; set; } = ""; // ✅ NEW

        public int DivSectionCode { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public DateTime? Date { get; set; }
        public bool Status { get; set; }
    }
}
