namespace BBMB.BudgetService.Features.CRManagements.GetById.Dtos
{
    public class GetCRManagementByIdResponseDto
    {
        public int CEId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public DateTime? Date { get; set; }
        public bool Status { get; set; }
        public string UserCode { get; set; } = string.Empty;
    }
}
