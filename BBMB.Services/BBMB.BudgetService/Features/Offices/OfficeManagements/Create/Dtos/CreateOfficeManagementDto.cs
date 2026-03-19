namespace BBMB.BudgetService.Features.Offices.OfficeManagements.Create.Dtos
{
    public class CreateOfficeManagementDto
    {
        public string OfficeCode { get; set; }
        public string OfficeDesc { get; set; }
        public int CECode { get; set; }
        public int? CRCode { get; set; }
        public int? DivisionCode { get; set; }
        public int LevelId { get; set; }
        public string? UserCode { get; set; }
    }
}
