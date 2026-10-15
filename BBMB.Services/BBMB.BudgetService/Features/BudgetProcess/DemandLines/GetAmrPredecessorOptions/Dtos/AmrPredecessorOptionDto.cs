namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetAmrPredecessorOptions.Dtos
{
    /// <summary>An AM&amp;R work of the preceding year that Table 3 column 14 may name (BUD-275).</summary>
    public sealed record AmrPredecessorOptionDto(int WorkId, int WorkCode, string WorkDesc);
}
