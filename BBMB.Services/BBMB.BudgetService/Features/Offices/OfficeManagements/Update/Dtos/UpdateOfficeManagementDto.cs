namespace BBMB.BudgetService.Features.Offices.OfficeManagements.Update.Dtos
{
    public class UpdateOfficeManagementDto
    {
        public string OfficeCode { get; set; }
        public string OfficeDesc { get; set; }
        public int CECode { get; set; }
        public int? CRCode { get; set; }
        public int LevelId { get; set; }
        public int? DivisionCode { get; set; }
    }
}
