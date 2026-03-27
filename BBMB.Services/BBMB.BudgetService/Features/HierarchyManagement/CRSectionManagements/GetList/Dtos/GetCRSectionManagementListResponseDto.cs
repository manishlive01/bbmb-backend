namespace BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.GetList.Dtos
{
    public class GetCRSectionManagementListResponseDto
    {
        public int Id { get; set; }
        public int CRId { get; set; }

        public int CRCode { get; set; }          // ✅ NEW
        public string CRName { get; set; } = ""; // ✅ NEW
        public int CRSectionCode { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public DateTime? Date { get; set; }
        public bool Status { get; set; }
    }
}
