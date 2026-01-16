namespace BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.GetList.Dtos
{
    public class GetSubMinorHeadListResponseDto
    {
        public string MinorAccCode { get; set; } = string.Empty;
        public string SubMinorAccCode { get; set; } = string.Empty;
        public string SubMinorAccDesc { get; set; } = string.Empty;

        // From parent Minor Head
        public string MinorAccDesc { get; set; } = string.Empty;

        // Formatted date for UI
        public string ModificationDate { get; set; } = string.Empty;

        public string UserCode { get; set; } = string.Empty;
    }
}
