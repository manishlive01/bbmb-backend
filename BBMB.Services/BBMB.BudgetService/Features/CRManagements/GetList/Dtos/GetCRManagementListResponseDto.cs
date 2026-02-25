namespace BBMB.BudgetService.Features.CRManagements.GetList.Dtos
{
    public class GetCRManagementListResponseDto
    {
        public string CRCode { get; set; } = string.Empty;
        public string CECode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public DateTime? Date { get; set; }
        public bool Status { get; set; }
        public string UserCode { get; set; } = string.Empty;
    }
}
