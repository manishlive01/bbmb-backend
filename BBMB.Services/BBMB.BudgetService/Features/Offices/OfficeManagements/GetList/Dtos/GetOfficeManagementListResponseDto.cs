namespace BBMB.BudgetService.Features.Offices.OfficeManagements.GetList.Dtos
{
    public class GetOfficeManagementListResponseDto
    {
        public string OfficeCode { get; set; }
        public string OfficeDesc { get; set; }

        public int CECode { get; set; }
        public string CEName { get; set; }

        public int? CRCode { get; set; }
        public string? CRName { get; set; }

        public int? DivisionCode { get; set; }
        public string? DivName { get; set; }

        public DateTime? ModificationDate { get; set; }
        public string? UserCode { get; set; }
    }
}
