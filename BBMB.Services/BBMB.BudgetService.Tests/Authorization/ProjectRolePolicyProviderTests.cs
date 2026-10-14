using Auth.Authorization.PolicyProvider;
using Auth.Authorization.Requirements;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace BBMB.BudgetService.Tests.Authorization;

public sealed class ProjectRolePolicyProviderTests
{
    private static ProjectRolePolicyProvider Provider() => new(Options.Create(new AuthorizationOptions()));

    [Fact]
    public async Task BUD033_BudgetRight_policy_holds_only_the_key_requirement()
    {
        var policy = await Provider().GetPolicyAsync("BudgetRight:F07.Submit");
        policy.Should().NotBeNull();
        policy!.Requirements.Should().ContainSingle()
            .Which.Should().Be(new BudgetRightRequirement("F07.Submit"));
    }

    [Fact]
    public async Task Legacy_Budget_roles_format_is_unchanged()
    {
        var policy = await Provider().GetPolicyAsync("Budget:Admin,BudgetViewer");
        var req = policy!.Requirements.Should().ContainSingle().Which.Should().BeOfType<ProjectRoleRequirement>().Subject;
        req.ProjectName.Should().Be("Budget");
        req.AllowedRoles.Should().Equal("Admin", "BudgetViewer");
    }

    [Fact]
    public async Task BudgetRight_without_key_has_no_policy()
    {
        (await Provider().GetPolicyAsync("BudgetRight:")).Should().BeNull();
    }
}
