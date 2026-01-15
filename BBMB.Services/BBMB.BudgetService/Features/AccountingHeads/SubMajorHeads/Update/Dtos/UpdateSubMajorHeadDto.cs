namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Update.Dtos
{
    public class UpdateSubMajorHeadDto
    {
        public string MajorAccCode { get; set; } = string.Empty;
        public string SubMajorAccCode { get; set; } = string.Empty;
        public string SubMajorAccDesc { get; set; } = string.Empty;
    }
}
