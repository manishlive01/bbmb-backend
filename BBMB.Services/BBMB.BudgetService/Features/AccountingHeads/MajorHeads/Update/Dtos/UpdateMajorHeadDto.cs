namespace BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Update.Dtos
{
    public class UpdateMajorHeadDto
    {
        public string MajorAccCode { get; set; } = string.Empty;
        public string MajorAccDesc { get; set; } = string.Empty;
        public DateTime ModificationDate { get; set; }
        public string UserCode { get; set; } = "123";
    }
}
