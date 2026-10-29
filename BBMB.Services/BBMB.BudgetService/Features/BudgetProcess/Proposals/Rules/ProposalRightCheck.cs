using BBMB.BudgetService.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules
{
    /// <summary>
    /// "Does the current user hold this Budget right?" for <see cref="ProposalTransitionService"/>. Groups the two
    /// dependencies that only ever served this one question (<see cref="IAuthorizationService"/> and the request's
    /// user from <see cref="IHttpContextAccessor"/>). The check is the same policy evaluation as before:
    /// <c>BudgetRight:&lt;key&gt;</c> through <c>BudgetRightHandler</c>, against the acting role; with no
    /// request, an anonymous principal (which holds nothing).
    /// </summary>
    public sealed class ProposalRightCheck(IAuthorizationService authorization, IHttpContextAccessor httpContextAccessor)
    {
        public async Task<bool> HoldsAsync(string rightKey)
        {
            var user = httpContextAccessor.HttpContext?.User ?? new System.Security.Claims.ClaimsPrincipal();
            return (await authorization.AuthorizeAsync(user, null, BudgetRightRequirement.Policy(rightKey))).Succeeded;
        }
    }
}
