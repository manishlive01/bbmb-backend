namespace BBMB.BudgetService.Features.Offices.RemittanceOffices.GetById.Dtos
{
    public class GetRemittanceByIdDto
    {
        public string RemittanceCode { get; set; } = string.Empty;
        public string RemittanceDesc { get; set; } = string.Empty;
        public DateTime ModificationDate { get; set; }
    }
}
