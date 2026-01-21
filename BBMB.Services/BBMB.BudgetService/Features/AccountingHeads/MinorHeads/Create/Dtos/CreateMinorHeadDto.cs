namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Create.Dtos
{
    public class CreateMinorHeadDto
    {
        public string MinorAccCode { get; set; } = string.Empty;
        public string MinorAccDesc { get; set; } = string.Empty;
        //public string MajorAccCode { get; set; } = string.Empty;
        //public string SubMajorAccCode { get; set; } = string.Empty;
        public string UserCode { get; set; }= "123";
    }
}
