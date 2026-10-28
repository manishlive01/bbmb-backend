using System.Security.Claims;
using BBMB.BudgetService.Authorization.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.AuthzConfig;

namespace BBMB.BudgetService.Authorization.RoleSource
{
    /// <summary>
    /// Reads the user's Budget-project roles from the token's <c>projects</c> claim (requirement 2), matching
    /// the Budget project by the <c>SsoProjectId</c> first and the <c>SsoProjectName</c> as a fallback, both
    /// read from the active <c>budget_project_settings</c> row via <see cref="IBudgetConfigProvider"/> rather
    /// than a hard-coded <c>"Budget"</c> string. Roles under <c>Budget-Staging</c>, <c>SSO-Auth</c> or any
    /// other project are never returned.
    /// </summary>
    public sealed class TokenClaimsRoleSource(IBudgetConfigProvider config) : IBudgetRoleSource
    {
        public async Task<IReadOnlyList<string>> GetBudgetRolesAsync(ClaimsPrincipal user, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(user);
            var snapshot = await config.GetAsync(ct);
            if (string.IsNullOrWhiteSpace(snapshot.SsoProjectName) && snapshot.SsoProjectId is null)
                return [];
            return ProjectClaimReader.RolesInProject(user, snapshot.SsoProjectId, snapshot.SsoProjectName);
        }
    }
}
