using System.Net;

namespace BBMB.BudgetService.Shared.Services.BudgetActors
{
    /// <summary>
    /// Who is acting, resolved once per request from the session (never from the request body, DDD §7.1;
    /// design §4.3). <see cref="RoleCode"/> is the acting role authorised by <c>BudgetRightHandler</c>.
    /// </summary>
    public sealed record BudgetActor(
        Guid EmployeeId,
        Guid ChargeId,
        long OfficeId,
        string OfficeLevel,
        string RoleCode,
        string ActorName,
        string? OnBehalfOfPost,
        IPAddress ClientIp,
        Guid RequestId);

    /// <summary>
    /// Result of resolving the actor. <see cref="Actor"/> is null when no actor resolves (the handler answers
    /// M-20); <see cref="AmbiguousChargeIds"/> lists the charges when the employee holds several current Budget
    /// charges and named none (design §4.3, D-12), so the client can ask the user.
    /// </summary>
    public sealed record BudgetActorResolution(BudgetActor? Actor, IReadOnlyList<Guid> AmbiguousChargeIds, string? Reason)
    {
        public static BudgetActorResolution None(string reason) => new(null, [], reason);
    }
}
