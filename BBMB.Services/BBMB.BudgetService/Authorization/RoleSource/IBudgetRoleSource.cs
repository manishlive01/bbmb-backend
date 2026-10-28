using System.Security.Claims;

namespace BBMB.BudgetService.Authorization.RoleSource
{
    /// <summary>
    /// The source of a user's Budget-project roles (requirement 2). Today the only implementation reads the
    /// token's <c>projects</c> claim; a future implementation will call an Auth SSO API that returns the
    /// user's complete role list grouped by project. Authorization logic throughout Budget endpoints depends
    /// only on this interface, so the source can switch in DI with no endpoint or handler change.
    /// <para>
    /// Only roles under the Budget project (matched by the <c>SsoProjectId</c>/<c>SsoProjectName</c> stored in
    /// <c>budget_project_settings</c>) are ever returned. Roles held in another project grant nothing. No
    /// user-to-role assignment is stored in Budget tables — Auth SSO remains the source.
    /// </para>
    /// </summary>
    public interface IBudgetRoleSource
    {
        /// <summary>The distinct Budget-project role names the user holds, or an empty list.</summary>
        Task<IReadOnlyList<string>> GetBudgetRolesAsync(ClaimsPrincipal user, CancellationToken ct = default);
    }
}
