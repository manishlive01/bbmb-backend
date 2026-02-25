namespace BBMB.BudgetService.Features.CRManagements.Update.Dtos
{
    public class UpdateCRManagementDto
    {
        public string CECode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public bool Status { get; set; }
    }
}
