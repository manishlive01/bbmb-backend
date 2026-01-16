namespace BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.Create.Dtos
{
    public class CreateSubMinorHeadDto
    {
        public string MinorAccCode { get; set; } = string.Empty;
        public string SubMinorAccCode { get; set; } = string.Empty;
        public string SubMinorAccDesc { get; set; } = string.Empty;
    }
}
