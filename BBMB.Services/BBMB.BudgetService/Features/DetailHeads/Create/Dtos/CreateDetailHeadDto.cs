namespace BBMB.BudgetService.Features.DetailHeads.Create.Dtos
{
    public class CreateDetailHeadDto
    {
        public string DetailAccCode { get; set; } = string.Empty;
        public string DetailAccDesc { get; set; } = string.Empty;
        public string MajorAccCode { get; set; } = string.Empty;
        public string SubMajorAccCode { get; set; } = string.Empty;
        public string MinorAccCode { get; set; } = string.Empty;
        public string SubMinorAccCode { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
    }
}
