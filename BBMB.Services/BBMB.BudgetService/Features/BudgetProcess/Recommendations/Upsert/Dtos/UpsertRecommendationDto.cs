namespace BBMB.BudgetService.Features.BudgetProcess.Recommendations.Upsert.Dtos
{
    /// <summary>Design §8.3 <c>PUT /api/budget-cycles/{cycleId}/recommendations</c> body (F-13 columns 9 and 10, T-37).</summary>
    public sealed class UpsertRecommendationDto
    {
        /// <summary>DNA, RECEIPTS or CAPEX (head-wise only, T-37 CHECK).</summary>
        public string Category { get; set; } = string.Empty;
        /// <summary>A CE office (BUD-303, DB-01).</summary>
        public long CeOfficeId { get; set; }
        public long AccountCodeId { get; set; }
        /// <summary>Column 9, whole rupees.</summary>
        public decimal? RbeRecommended { get; set; }
        /// <summary>Column 10, whole rupees.</summary>
        public decimal? ObeNextRecommended { get; set; }
        /// <summary>The concurrence of the FA &amp; CAO; required where a figure is set (BUD-153, BUD-154).</summary>
        public string? ConcurrenceRef { get; set; }
    }
}
