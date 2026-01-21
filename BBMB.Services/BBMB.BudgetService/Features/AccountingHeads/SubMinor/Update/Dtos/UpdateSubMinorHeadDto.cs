namespace BBMB.BudgetService.Features.AccountingHeads.SubMinor.Update.Dtos
{
    public class UpdateSubMinorHeadDto
    {
        public string SubMinorAccDesc { get; set; } = string.Empty;
        //public string MajorAccCode { get; set; } = string.Empty;
        //public string SubMajorAccCode { get; set; } = string.Empty;
        public string MinorAccCode { get; set; } = string.Empty;
        public string UserCode { get; set; } = "123";
    }
}
