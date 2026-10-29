using System.Security.Claims;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules;
using BBMB.BudgetService.Shared.Services.Notifications;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
namespace BBMB.BudgetService.Tests.Features.BudgetProcess.Proposals.Rules;
/// <summary>
/// The two collaborators extracted from <see cref="ProposalTransitionService"/>'s constructor. They carry the
/// exact logic the service ran inline, so these tests pin it without a database; the service's own move tests
/// (PostgreSQL, <see cref="ProposalTransitionServiceTests"/>) run unchanged through <see cref="TestServices.Transitions"/>.
/// </summary>
public sealed class ProposalTransitionCollaboratorsTests
{
    private sealed class RecordingAuthorization(bool allow) : IAuthorizationService
    {
        public ClaimsPrincipal? User { get; private set; }
        public string? Policy { get; private set; }
        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, IEnumerable<IAuthorizationRequirement> requirements) =>
            Task.FromResult(AuthorizationResult.Failed());
        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, string policyName)
        {
            (User, Policy) = (user, policyName);
            return Task.FromResult(allow ? AuthorizationResult.Success() : AuthorizationResult.Failed());
        }
    }

    [Fact]
    public async Task The_right_check_evaluates_the_BudgetRight_policy_for_the_request_user()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "u1")], "Bearer"));
        var auth = new RecordingAuthorization(allow: true);
        var check = new ProposalRightCheck(auth, new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = user } });

        (await check.HoldsAsync("F07.Submit")).Should().BeTrue();
        auth.Policy.Should().Be("BudgetRight:F07.Submit");
        auth.User.Should().BeSameAs(user);
    }

    [Fact]
    public async Task The_right_check_refuses_when_the_policy_fails_and_uses_an_anonymous_user_without_a_request()
    {
        var auth = new RecordingAuthorization(allow: false);
        var check = new ProposalRightCheck(auth, new HttpContextAccessor());

        (await check.HoldsAsync("Review.Examine")).Should().BeFalse();
        auth.User!.Identity.Should().BeNull("no request: an anonymous principal, which holds nothing");
    }

    [Fact]
    public async Task The_right_check_matches_the_fake_used_by_the_transition_tests()
    {
        var check = new ProposalRightCheck(TestServices.Allow("F07.Submit"), new HttpContextAccessor { HttpContext = new DefaultHttpContext() });

        (await check.HoldsAsync("F07.Submit")).Should().BeTrue();
        (await check.HoldsAsync("F30.Authorise")).Should().BeFalse();
    }

    private sealed class ThrowingGateway : IBudgetNotificationGateway
    {
        public int Calls { get; private set; }
        public Task NotifyMovementAsync(long proposalId, long movementId, CancellationToken ct = default)
        {
            Calls++;
            throw new InvalidOperationException("gateway down");
        }
    }

    [Fact]
    public async Task A_notification_failure_never_fails_the_committed_move()
    {
        var gateway = new ThrowingGateway();
        var notifier = new ProposalMovementNotifier(gateway, NullLogger<ProposalMovementNotifier>.Instance);

        var act = () => notifier.NotifyAsync(1, 2, default);

        await act.Should().NotThrowAsync();
        gateway.Calls.Should().Be(1);
    }
}
