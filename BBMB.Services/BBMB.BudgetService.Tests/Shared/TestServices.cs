using BBMB.BudgetService.Authorization.Requirements;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using BBMB.BudgetService.Shared.Services.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BBMB.BudgetService.Tests.Shared;

/// <summary>Builds the budget-process services by hand, as handler tests instantiate handlers (design §14).</summary>
public static class TestServices
{
    /// <summary>Every right key of the default BudgetRights section.</summary>
    public static readonly string[] AllRights =
    [
        "F01.Create", "F01.MarkNotForUse", "F17.Assign", "F10.Act", "F13.Act", "F19.Allocate", "F20.Freeze", "F21.Enter",
        "F23.Set", "Cycle.Manage", "Settings.Manage", "Finance.Open", "F01.FlagPartyEmp", "Settings.ExpenditureSource",
        "Offices.Load", "F03.Enter", "F06.Enter", "F07.Enter", "F07.Submit", "F29.Allocate", "F30.Submit", "Works.Enter",
        "F11.Enter", "F11.Submit", "Review.Examine", "Review.ForwardFinance", "F14.Distribute", "F30.Authorise",
        "F30.Examine", "Proposal.Recall", "Proposal.Reply", "Budget.View", "Legacy.BudgetEntry", "Attachment.Upload",
    ];

    public static FakeAuthorizationService AllowAll() =>
        new(AllRights.Select(BudgetRightRequirement.Policy).ToArray());

    public static FakeAuthorizationService Allow(params string[] rightKeys) =>
        new(rightKeys.Select(BudgetRightRequirement.Policy).ToArray());

    private sealed class StaticMonitor(BudgetRightsOptions value) : IOptionsMonitor<BudgetRightsOptions>
    {
        public BudgetRightsOptions CurrentValue => value;
        public BudgetRightsOptions Get(string? name) => value;
        public IDisposable? OnChange(Action<BudgetRightsOptions, string?> listener) => null;
    }

    public static IOptionsMonitor<BudgetRightsOptions> RightsOptions() => new StaticMonitor(new BudgetRightsOptions
    {
        RoleOfficeLevels = new(StringComparer.OrdinalIgnoreCase)
        {
            ["DDO"] = "DIVISION", ["SE"] = "CIRCLE", ["CE"] = "CE", ["SECRETARY"] = "SECRETARIAT",
            ["AFA_BUDGET"] = "FINANCE", ["FIN_VIEW"] = "FINANCE", ["IT_CELL"] = "IT_CELL",
        },
    });

    public static IBudgetOfficeScope OfficeScope(BudgetContext ctx) => new BudgetOfficeScope(ctx, RightsOptions());

    public static ProposalTransitionService Transitions(
        BudgetContext ctx, IBudgetActorSource actor, FakeAuthorizationService? authorization = null) =>
        new(ctx, actor, new BudgetActionScope(ctx, actor), OfficeScope(ctx), authorization ?? AllowAll(),
            new HttpContextAccessor { HttpContext = new DefaultHttpContext() }, new NoOpBudgetNotificationGateway(),
            NullLogger<ProposalTransitionService>.Instance);
}
