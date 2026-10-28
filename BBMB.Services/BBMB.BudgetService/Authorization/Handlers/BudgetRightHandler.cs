using Auth.Authorization.Requirements;
using BBMB.BudgetService.Authorization.Dtos;
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
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            BudgetRightRequirement requirement)
        {
            var current = options.CurrentValue;
            var httpContext = httpContextAccessor.HttpContext;

            var actingRole = ResolveActingRole(context, current, httpContext, out var reason);
            if (actingRole is null)
            {
                logger.LogDebug("BudgetRight {RightKey} refused: {Reason}", requirement.RightKey, reason);
                return Task.CompletedTask;
            }

            if (!current.Rights.TryGetValue(requirement.RightKey, out var roles))
            {
                logger.LogWarning("BudgetRight {RightKey} refused: unknown right key", requirement.RightKey);
                return Task.CompletedTask;
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
            return Task.CompletedTask;
        }

        private static string? ResolveActingRole(
            AuthorizationHandlerContext context, BudgetRightsOptions current, HttpContext? httpContext, out string reason)
        {
            if (BudgetActingRole.Get(httpContext) is { } resolved)
            {
                reason = "REUSED";
                return resolved;
            }

            // Only configured Budget roles and explicit administrators count. Other project roles do not.
            var configuredRoles = current.RoleOfficeLevels.Keys
                .Concat(current.AdminRoles)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var held = ProjectClaimReader.RolesIn(context.User, current.ProjectName)
                .Where(r => configuredRoles.Count == 0 || configuredRoles.Contains(r))
                .ToList();
            if (held.Count == 0)
            {
                reason = "NO_BUDGET_ROLE";
                return null;
            }

            string? acting;
            var header = httpContext?.Request.Headers[BudgetActingRole.HeaderName].ToString();
            if (!string.IsNullOrWhiteSpace(header))
            {
                acting = held.FirstOrDefault(r => string.Equals(r, header.Trim(), StringComparison.OrdinalIgnoreCase));
                if (acting is null)
                {
                    reason = "HEADER_ROLE_NOT_HELD";
                    return null;
                }
            }
            else if (held.Count == 1)
            {
                acting = held[0];
            }
            else
            {
                reason = "SEVERAL_ROLES_NO_HEADER";
                return null;
            }

            if (httpContext is not null)
                httpContext.Items[BudgetActingRole.ItemKey] = acting;
            reason = "RESOLVED";
            return acting;
        }
    }
}
