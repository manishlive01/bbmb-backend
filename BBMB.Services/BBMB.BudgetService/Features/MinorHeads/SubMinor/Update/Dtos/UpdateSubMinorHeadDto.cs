namespace BBMB.BudgetService.Features.MinorHeads.SubMinor.Update.Dtos
{
    public class UpdateSubMinorHeadDto
    {
        public string SubMinorAccDesc { get; set; } = string.Empty;
        public string MajorAccCode { get; set; } = string.Empty;
        public string SubMajorAccCode { get; set; } = string.Empty;
        public string MinorAccCode { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
    }
}
