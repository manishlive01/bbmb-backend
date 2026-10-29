namespace BBMB.BudgetService.Authorization.Handlers
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

        /// <summary>
        /// Key under which <see cref="BudgetRightHandler"/> stores the role-selection outcome
        /// (<see cref="RoleSelection"/>) when authorization fails specifically on acting-role selection, so the
        /// terminal response can return 409 with the selectable roles (requirement 3, design §4).
        /// </summary>
        public const string SelectionItemKey = "BBMB.Budget.RoleSelection";

        /// <summary>Request header naming the acting role when the user holds several Budget roles.</summary>
        public const string HeaderName = "X-Budget-Role";

        public static string? Get(HttpContext? httpContext) =>
            httpContext?.Items.TryGetValue(ItemKey, out var v) == true ? v as string : null;

        public static RoleSelection? GetSelection(HttpContext? httpContext) =>
            httpContext?.Items.TryGetValue(SelectionItemKey, out var v) == true ? v as RoleSelection : null;
    }

    /// <summary>
    /// The reason an acting role could not be resolved, and the roles the user could select from, so the
    /// terminal response can prompt for an <c>X-Budget-Role</c> header (requirement 3). Only the selection
    /// reasons (several-held, or an invalid header value) carry a non-empty list; a genuine lack of any
    /// Budget role is not a selection prompt.
    /// </summary>
    public sealed record RoleSelection(string Reason, IReadOnlyList<string> SelectableRoles)
    {
        public const string SeveralRolesNoHeader = "SEVERAL_ROLES_NO_HEADER";
        public const string HeaderRoleNotHeld = "HEADER_ROLE_NOT_HELD";

        /// <summary>True when the reason is one the frontend can resolve by sending a role header.</summary>
        public bool IsSelectable => Reason is SeveralRolesNoHeader or HeaderRoleNotHeld;
    }
}
