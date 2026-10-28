using Auth.Authorization.Requirements;
using BBMB.BudgetService.Authorization.Dtos;
using BBMB.BudgetService.Authorization.RoleSource;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Auth.Authorization.Handlers
{
    /// <summary>
    /// Evaluates <see cref="BudgetRightRequirement"/> against the <b>acting role only</b> (design §4.4):
    /// <list type="number">
    /// <item>read the roles held in the Budget project from the <c>projects</c> claim;</item>
    /// <item>acting role = header <c>X-Budget-Role</c> if present and held; else the single held role; else fail;</item>
    /// <item>store it in <c>HttpContext.Items</c> (a second evaluation in the request reuses it);</item>
    /// <item>succeed only if the acting role is listed for the key in <see cref="BudgetRightsOptions"/>,
    /// read at evaluation time. An unknown key fails closed.</item>
    /// </list>
    /// Unlike <c>ProjectRoleHandler</c>, holding <i>some</i> role with the right is not enough.
    /// </summary>
    public sealed class BudgetRightHandler(
        IOptionsMonitor<BudgetRightsOptions> options,
        IHttpContextAccessor httpContextAccessor,
        ILogger<BudgetRightHandler> logger)
        : AuthorizationHandler<BudgetRightRequirement>
    {
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            BudgetRightRequirement requirement)
        {
            var current = options.CurrentValue;
            var httpContext = httpContextAccessor.HttpContext;

            var (actingRole, reason) = await ResolveActingRoleAsync(context, current, httpContext);
            if (actingRole is null)
            {
                logger.LogDebug("BudgetRight {RightKey} refused: {Reason}", requirement.RightKey, reason);
                return;
            }

            if (!current.Rights.TryGetValue(requirement.RightKey, out var roles))
            {
                logger.LogWarning("BudgetRight {RightKey} refused: unknown right key", requirement.RightKey);
                return;
            }

            if (current.AdminRoles.Contains(actingRole, StringComparer.OrdinalIgnoreCase) ||
                roles.Contains(actingRole, StringComparer.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
            }
            else
            {
                logger.LogDebug("BudgetRight {RightKey} refused: ROLE_LACKS_RIGHT", requirement.RightKey);
            }
        }

        private async Task<(string? Role, string Reason)> ResolveActingRoleAsync(
            AuthorizationHandlerContext context, BudgetRightsOptions current, HttpContext? httpContext)
        {
            if (BudgetActingRole.Get(httpContext) is { } resolved)
                return (resolved, "REUSED");

            // The role source (requirement 2) is the only reader of the projects claim: Budget-project roles
            // only, matched by the stored ProjectId/ProjectName. It is scoped, so reach it from the request
            // services (the handler is a singleton).
            IReadOnlyList<string> budgetRoles;
            if (httpContext?.RequestServices.GetService(typeof(IBudgetRoleSource)) is IBudgetRoleSource roleSource)
                budgetRoles = await roleSource.GetBudgetRolesAsync(context.User, httpContext.RequestAborted);
            else
                budgetRoles = ProjectClaimReader.RolesIn(context.User, current.ProjectName);

            // Only configured Budget roles and explicit administrators count.
            var configuredRoles = current.RoleOfficeLevels.Keys
                .Concat(current.AdminRoles)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var held = budgetRoles
                .Where(r => configuredRoles.Count == 0 || configuredRoles.Contains(r))
                .ToList();
            if (held.Count == 0)
                return (null, "NO_BUDGET_ROLE");

            string? acting;
            var header = httpContext?.Request.Headers[BudgetActingRole.HeaderName].ToString();
            if (!string.IsNullOrWhiteSpace(header))
            {
                acting = held.FirstOrDefault(r => string.Equals(r, header.Trim(), StringComparison.OrdinalIgnoreCase));
                if (acting is null)
                {
                    RecordSelection(httpContext, RoleSelection.HeaderRoleNotHeld, held);
                    return (null, RoleSelection.HeaderRoleNotHeld);
                }
            }
            else if (held.Count == 1)
            {
                acting = held[0];
            }
            else
            {
                RecordSelection(httpContext, RoleSelection.SeveralRolesNoHeader, held);
                return (null, RoleSelection.SeveralRolesNoHeader);
            }

            if (httpContext is not null)
                httpContext.Items[BudgetActingRole.ItemKey] = acting;
            return (acting, "RESOLVED");
        }

        /// <summary>
        /// Stores the selectable-role list so a terminal response can return 409 with a prompt (requirement 3,
        /// design §4). Never selects a role by precedence; a role from another project is never here.
        /// </summary>
        private static void RecordSelection(HttpContext? httpContext, string reason, IReadOnlyList<string> selectable)
        {
            if (httpContext is not null)
                httpContext.Items[BudgetActingRole.SelectionItemKey] = new RoleSelection(reason, selectable);
        }
    }
}
