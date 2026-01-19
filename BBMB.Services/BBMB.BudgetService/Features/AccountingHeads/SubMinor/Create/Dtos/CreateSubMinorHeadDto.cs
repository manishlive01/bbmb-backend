namespace BBMB.BudgetService.Features.AccountingHeads.SubMinor.Create.Dtos
{
    public class CreateSubMinorHeadDto
    {
        public string SubMinorAccCode { get; set; } = string.Empty;
        public string SubMinorAccDesc { get; set; } = string.Empty;
        //public string MajorAccCode { get; set; } = string.Empty;
        //public string SubMajorAccCode { get; set; } = string.Empty;
        public string MinorAccCode { get; set; } = string.Empty;
        public string UserCode { get; set; } = "123";
    }
}
