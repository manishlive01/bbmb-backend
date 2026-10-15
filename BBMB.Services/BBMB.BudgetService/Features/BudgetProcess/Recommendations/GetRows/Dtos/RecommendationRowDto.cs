namespace BBMB.BudgetService.Features.BudgetProcess.Recommendations.GetRows.Dtos
{
    /// <summary>
    /// One F-13 row per (CE, account code) (BUD-302, BUD-304): demanded columns 7 and 8 (V-03 totals of the CE
    /// and the offices under it), recommended columns 9 and 10 with the concurrence, and the drill-down keys.
    /// </summary>
    public sealed class RecommendationRowDto
    {
        public long CeOfficeId { get; set; }
        public string? CeOfficeName { get; set; }
        public long AccountCodeId { get; set; }
        public string? CompleteCode { get; set; }
        public string? HeadDesc { get; set; }
        public decimal? RbeDemanded { get; set; }
        public decimal? ObeNextDemanded { get; set; }
        public int LineCount { get; set; }
        public int OfficeCount { get; set; }
        public long? RecommendationId { get; set; }
        public decimal? RbeRecommended { get; set; }
        public decimal? ObeNextRecommended { get; set; }
        public string? ConcurrenceRef { get; set; }
    }
}
