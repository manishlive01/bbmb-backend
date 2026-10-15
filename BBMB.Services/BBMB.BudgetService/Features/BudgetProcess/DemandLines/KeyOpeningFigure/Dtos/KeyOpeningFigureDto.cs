namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.KeyOpeningFigure.Dtos
{
    /// <summary>Design §8.3 <c>POST …/demand-lines/{lineId}/opening-figures</c> body (T-36).</summary>
    public sealed class KeyOpeningFigureDto
    {
        /// <summary>
        /// OBE_CURRENT (Table 3 col 9, Table 4 col 5) or RBE_PREVIOUS (Table 4 col 3, head-wise lines only).
        /// </summary>
        public string FigureKind { get; set; } = string.Empty;
        /// <summary>Whole rupees (budget.rupees).</summary>
        public decimal Amount { get; set; }
    }
}
