namespace Auth.Authorization.Handlers
{
    /// <summary>
    /// The acting role resolved by <see cref="BudgetRightHandler"/> for the current request (design §4.4).
    /// The same value is what the office is resolved from and what T-51 <c>actor_role_code</c> records, so
    /// right, office and audited role cannot disagree within one request (BUD-034).
    /// </summary>
    public static class BudgetActingRole
    {
        /// <summary>Key of the resolved role in <c>HttpContext.Items</c>.</summary>
        public const string ItemKey = "BBMB.Budget.ActingRole";

        /// <summary>Request header naming the acting role when the user holds several Budget roles.</summary>
        public const string HeaderName = "X-Budget-Role";

        public static string? Get(HttpContext? httpContext) =>
            httpContext?.Items.TryGetValue(ItemKey, out var v) == true ? v as string : null;
    }
}
