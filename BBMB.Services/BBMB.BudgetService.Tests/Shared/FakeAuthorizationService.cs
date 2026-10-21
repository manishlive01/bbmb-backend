using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace BBMB.BudgetService.Tests.Shared;

/// <summary>
/// Plan decision PD-4: handler-level right checks go through <see cref="IAuthorizationService"/>.
/// This fake grants exactly the policy names it is given and refuses every other.
/// </summary>
public sealed class FakeAuthorizationService(params string[] allowedPolicies) : IAuthorizationService
{
    private readonly HashSet<string> _allowed = new(allowedPolicies, StringComparer.Ordinal);

    public IReadOnlyCollection<string> Allowed => _allowed;

    public Task<AuthorizationResult> AuthorizeAsync(
        ClaimsPrincipal user, object? resource, IEnumerable<IAuthorizationRequirement> requirements) =>
        Task.FromResult(AuthorizationResult.Failed());

    public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, string policyName) =>
        Task.FromResult(_allowed.Contains(policyName)
            ? AuthorizationResult.Success()
            : AuthorizationResult.Failed());
}
