namespace BBMB.BudgetService.Features.MinorHeads.Update.Dtos
{
    public class UpdateMinorHeadDto
    {
        public string MinorAccDesc { get; set; } = string.Empty;
        public string MajorAccCode { get; set; } = string.Empty;
        public string SubMajorAccCode { get; set; } = string.Empty;
        public string UserCode { get; set; } = "123";
    }
}
