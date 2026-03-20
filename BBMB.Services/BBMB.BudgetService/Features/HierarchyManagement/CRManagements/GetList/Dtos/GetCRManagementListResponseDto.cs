namespace BBMB.BudgetService.Features.HierarchyManagement.CRManagements.GetList.Dtos
{
    public class GetCRManagementListResponseDto
    {
        public int CRCode { get; set; } 
        public int CEId { get; set; } 
        public int CRId { get; set; } 
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public DateTime? Date { get; set; }
        public bool Status { get; set; }
        public string UserCode { get; set; } = string.Empty;
    }
}
