namespace BBMB.BudgetService.Features.BudgetProcess.Reports
{
    /// <summary>How a catalogued field may be used in a query (design §8.8, BUD-220 to BUD-224).</summary>
    public enum QueryFieldKind
    {
        /// <summary>A grouping / filtering dimension (text or integer key).</summary>
        Dimension,
        /// <summary>A rupee figure, summed in the result.</summary>
        Measure,
    }

    /// <summary>A business-name field and the single <c>v_report_demand</c> column it maps to.</summary>
    public sealed record QueryField(string Name, string Column, QueryFieldKind Kind, string Description);

    /// <summary>
    /// The query builder's whitelist (design §8.8, F-26). Business names map to columns of
    /// <c>budget.v_report_demand</c> through this dictionary and nowhere else: a field not in the catalogue is
    /// refused, so no client string ever reaches a column name or a table name (BUD-220, BUD-221). Filter values
    /// are always bound as parameters by the handler, never interpolated. The catalogue is the only surface the
    /// query builder exposes; extending the reportable set means adding a column to the view and a row here.
    /// </summary>
    public static class QueryFieldCatalogue
    {
        public static readonly IReadOnlyList<QueryField> Fields =
        [
            new("category", "category", QueryFieldKind.Dimension, "Budget category (WORKS, DNA, RECEIPTS, CAPEX)"),
            new("state", "state_code", QueryFieldKind.Dimension, "Proposal state"),
            new("finYear", "current_fin_year", QueryFieldKind.Dimension, "Financial year (closing year)"),
            new("wing", "wing_code", QueryFieldKind.Dimension, "Wing"),
            new("ce", "ce_id", QueryFieldKind.Dimension, "Chief Engineer office"),
            new("circle", "circle_id", QueryFieldKind.Dimension, "Circle"),
            new("division", "division_id", QueryFieldKind.Dimension, "Division"),
            new("lineType", "line_type", QueryFieldKind.Dimension, "Line type (WORK, HEAD, UNFORESEEN)"),
            new("accountCode", "complete_code", QueryFieldKind.Dimension, "Account code"),
            new("demandedRbe", "demanded_rbe", QueryFieldKind.Measure, "Demanded revised estimate (current year)"),
            new("demandedObeNext", "demanded_obe_next", QueryFieldKind.Measure, "Demanded original estimate (next year)"),
            new("distributedRbe", "distributed_rbe", QueryFieldKind.Measure, "Distributed share, current year"),
            new("distributedObeNext", "distributed_obe_next", QueryFieldKind.Measure, "Distributed share, next year"),
        ];

        private static readonly IReadOnlyDictionary<string, QueryField> ByName =
            Fields.ToDictionary(f => f.Name, f => f, StringComparer.Ordinal);

        /// <summary>The catalogued field, or null when the business name is not whitelisted (refused, BUD-220).</summary>
        public static QueryField? Resolve(string? name) =>
            name is not null && ByName.TryGetValue(name, out var f) ? f : null;

        /// <summary>
        /// BUD-221: whether the acting role may use the field, read from <c>BudgetRights:QueryFieldRoles</c>
        /// (configuration, BUD-033). An empty map allows every field (OI-26 not yet configured); otherwise a
        /// field not listed, or listed without the role, is refused.
        /// </summary>
        public static bool IsAllowed(QueryField field, string role, IReadOnlyDictionary<string, string[]> fieldRoles) =>
            fieldRoles.Count == 0
            || (fieldRoles.TryGetValue(field.Name, out var roles) && roles.Contains(role, StringComparer.OrdinalIgnoreCase));

        /// <summary>The catalogue narrowed to the acting role (BUD-221).</summary>
        public static IReadOnlyList<QueryField> ForRole(string role, IReadOnlyDictionary<string, string[]> fieldRoles) =>
            Fields.Where(f => IsAllowed(f, role, fieldRoles)).ToList();

        /// <summary>Resolve a business name the role may use; null when unknown or not allowed to the role (both M-22).</summary>
        public static QueryField? ResolveForRole(string? name, string role, IReadOnlyDictionary<string, string[]> fieldRoles) =>
            Resolve(name) is { } f && IsAllowed(f, role, fieldRoles) ? f : null;
    }
}
