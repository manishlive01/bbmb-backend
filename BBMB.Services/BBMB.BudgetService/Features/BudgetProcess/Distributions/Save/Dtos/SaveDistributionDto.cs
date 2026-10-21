namespace BBMB.BudgetService.Features.BudgetProcess.Distributions.Save.Dtos
{
    /// <summary>Design §8.5 <c>PUT /api/budget-cycles/{cycleId}/distributions</c> body (F-14, BUD-156, BUD-157).</summary>
    public sealed class SaveDistributionDto
    {
        public long RecommendationId { get; set; }
        public List<DistributionLineDto> Lines { get; set; } = [];
    }

    /// <summary>One office's distributed share (columns 9/10).</summary>
    public sealed class DistributionLineDto
    {
        public long OfficeId { get; set; }
        public decimal RbeAmount { get; set; }
        public decimal ObeNextAmount { get; set; }
    }
}
