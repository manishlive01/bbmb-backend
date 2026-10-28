namespace BBMB.BudgetService.Features.BudgetProcess.AuthzConfig
{
    /// <summary>
    /// An immutable projection of the seven authz-config tables (requirement 1), read through
    /// <see cref="IBudgetConfigProvider"/> and projected onto the legacy <c>BudgetRightsOptions</c> shape by
    /// <see cref="BudgetConfigOptionsMonitor"/> so <c>BudgetRightHandler</c> and <c>BudgetOfficeScope</c> are
    /// untouched. Built once per cache load (design §2).
    /// </summary>
    public sealed record BudgetConfigSnapshot
    {
        /// <summary>The Budget project's token <c>ProjectId</c> (null until confirmed, OQ-2).</summary>
        public Guid? SsoProjectId { get; init; }

        /// <summary>The Budget project's token <c>ProjectName</c> (formerly <c>BudgetRights.ProjectName</c>).</summary>
        public string SsoProjectName { get; init; } = string.Empty;

        /// <summary><c>employee_charge.project_code</c> of the Budget project (formerly <c>BudgetProcess:ProjectCode</c>).</summary>
        public string ProjectCode { get; init; } = string.Empty;

        /// <summary>Edit-lease length in minutes (formerly <c>BudgetProcess:EditLeaseMinutes</c>).</summary>
        public int EditLeaseMinutes { get; init; }

        /// <summary>Active role → office level (formerly <c>BudgetRights.RoleOfficeLevels</c>).</summary>
        public IReadOnlyDictionary<string, string> RoleOfficeLevels { get; init; } =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Right key → roles that hold it (formerly <c>BudgetRights.Rights</c>).</summary>
        public IReadOnlyDictionary<string, string[]> Rights { get; init; } =
            new Dictionary<string, string[]>(StringComparer.Ordinal);

        /// <summary>Query field → roles that may use it (formerly <c>BudgetRights.QueryFieldRoles</c>).</summary>
        public IReadOnlyDictionary<string, string[]> QueryFieldRoles { get; init; } =
            new Dictionary<string, string[]>(StringComparer.Ordinal);

        /// <summary>
        /// Effective-admin role set (design §1 rule): a role whose <c>is_admin</c> is true AND which has an
        /// APPROVED, document-backed admin assignment. Formerly <c>BudgetRights.AdminRoles</c>, now gated by the
        /// document-backed approval (requirement 4). A PENDING/REVOKED role is absent here.
        /// </summary>
        public string[] AdminRoles { get; init; } = [];

        /// <summary>The empty snapshot used when no active settings row exists (fails closed, design "Error handling").</summary>
        public static BudgetConfigSnapshot Empty { get; } = new();
    }
}
