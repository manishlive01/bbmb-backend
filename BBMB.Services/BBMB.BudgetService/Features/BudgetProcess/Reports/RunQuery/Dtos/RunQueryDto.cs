namespace BBMB.BudgetService.Features.BudgetProcess.Reports.RunQuery.Dtos
{
    /// <summary>Design §8.8 (F-26) query request. All names are business names resolved through the whitelist.</summary>
    public sealed class RunQueryDto
    {
        /// <summary>Measure fields to total (business names). Empty defaults to the demanded figures.</summary>
        public List<string> Fields { get; set; } = [];
        public List<QueryFilterDto> Filters { get; set; } = [];
        /// <summary>Dimension fields to group by (business names).</summary>
        public List<string> GroupBy { get; set; } = [];
        /// <summary>Optional financial year, including a closed one (BUD-222).</summary>
        public int? FinYear { get; set; }
    }

    /// <summary>A filter: a whitelisted dimension field, an operator, and a value bound as a parameter.</summary>
    public sealed class QueryFilterDto
    {
        public string Field { get; set; } = string.Empty;
        /// <summary>eq or ne only (a value is always a value, never SQL).</summary>
        public string Op { get; set; } = "eq";
        public string? Value { get; set; }
    }
}
