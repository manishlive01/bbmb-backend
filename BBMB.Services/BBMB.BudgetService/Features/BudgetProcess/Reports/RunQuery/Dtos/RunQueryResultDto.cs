namespace BBMB.BudgetService.Features.BudgetProcess.Reports.RunQuery.Dtos
{
    /// <summary>Design §8.8 (F-26): the query result — the business-name columns and the rows, in a deterministic order.</summary>
    public sealed record RunQueryResultDto(IReadOnlyList<string> Columns, IReadOnlyList<IReadOnlyList<object?>> Rows);
}
