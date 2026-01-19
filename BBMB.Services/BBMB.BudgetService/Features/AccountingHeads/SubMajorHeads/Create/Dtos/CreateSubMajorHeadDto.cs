namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Create.Dtos
{
    public class CreateSubMajorHeadDto
    {
        public string SubMajorAccCode { get; set; } = string.Empty;
        public string SubMajorAccDesc { get; set; } = string.Empty;
        public string MajorAccCode { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
    }
}
