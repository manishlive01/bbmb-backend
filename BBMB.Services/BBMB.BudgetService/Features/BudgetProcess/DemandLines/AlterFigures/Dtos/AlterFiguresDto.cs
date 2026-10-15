namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.AlterFigures.Dtos
{
    /// <summary>Design §8.3 <c>PUT …/demand-lines/{lineId}/figures</c> body (F-09, F-12, F-10).</summary>
    public sealed class AlterFiguresDto
    {
        /// <summary>The altered revised estimate; null leaves the figure as it stands.</summary>
        public decimal? RbeAmount { get; set; }
        /// <summary>The altered next-year estimate; null leaves the figure as it stands.</summary>
        public decimal? ObeNextAmount { get; set; }
        /// <summary>Recorded with the figure version (T-31), not on the line (BUD-122).</summary>
        public string? Justification { get; set; }
        /// <summary>The officer chose "Continue without justification" after the ADV-122 advice (BUD-122).</summary>
        public bool ContinueWithoutJustification { get; set; }
    }
}
