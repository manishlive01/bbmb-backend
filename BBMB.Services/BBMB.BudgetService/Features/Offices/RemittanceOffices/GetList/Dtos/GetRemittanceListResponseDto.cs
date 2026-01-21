namespace BBMB.BudgetService.Features.Offices.RemittanceOffices.GetList.Dtos
{
    public class GetRemittanceListResponseDto
    {
        public string ROCode { get; set; } = string.Empty;
        public string RODesc { get; set; } = string.Empty;
        public DateTime ModificationDate { get; set; }
        public string UserCode { get; set; } = string.Empty;
    }
}
