namespace BBMB.BudgetService.Features.Offices.AccountOffices.GetList.Dtos
{
    public class GetAccountOfficeListResponseDto
    {
        public string AccOffice { get; set; } = string.Empty;
        public string AccOfficeDesc { get; set; } = string.Empty;
        public DateTime ModificationDate { get; set; } = DateTime.UtcNow;
        public string UserCode { get; set; } = string.Empty;

    }
}
