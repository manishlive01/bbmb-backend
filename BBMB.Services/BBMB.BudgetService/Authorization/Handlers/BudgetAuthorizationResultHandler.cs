using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace BBMB.BudgetService.Authorization.Handlers
{
    /// <summary>
    /// Wraps the default authorization result handler to turn a forbidden decision that failed <b>specifically
    /// on acting-role selection</b> (requirement 3, design §4) into an explicit, machine-readable 409: the
    /// request is well-formed, it is the actor state that is ambiguous (matching the charge-ambiguity
    /// treatment), so 409 Conflict, not 403. The body carries message id <c>M-20</c> with <c>reason</c> and
    /// <c>selectableRoles</c> so the frontend can prompt the user and resend with <c>X-Budget-Role</c>.
    /// Every other forbidden/unauthorised decision is left to the default handler unchanged.
    /// </summary>
    public sealed class BudgetAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _default = new();

        public async Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            if (authorizeResult.Forbidden &&
                BudgetActingRole.GetSelection(context) is { IsSelectable: true } selection)
            {
                var body = new ActorSelectionResponse
                {
                    IsSucced = false,
                    MessageId = BudgetMessages.M20,
                    Message = BudgetMessages.Format(BudgetMessages.M20),
                    Reason = selection.Reason,
                    SelectableRoles = selection.SelectableRoles,
                };
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(body);
                return;
            }

            await _default.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}
