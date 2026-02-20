namespace BBMB.BudgetService.Features.CRManagements.Create.Dtos
{
    public class CreateCRManagementDto
    {
        public int CEId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public bool Status { get; set; }
    }

}
