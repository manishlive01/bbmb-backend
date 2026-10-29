using System.Security.Claims;
using System.Text.Json;
using BBMB.BudgetService.Authorization.Handlers;
using BBMB.BudgetService.Authorization.Requirements;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BBMB.BudgetService.Tests.Authorization;

/// <summary>Design §4.4 and §14 (handler unit list).</summary>
public sealed class BudgetRightHandlerTests
{
    private sealed class MutableMonitor(BudgetRightsOptions value) : IOptionsMonitor<BudgetRightsOptions>
    {
        public BudgetRightsOptions CurrentValue { get; set; } = value;
        public BudgetRightsOptions Get(string? name) => CurrentValue;
        public IDisposable? OnChange(Action<BudgetRightsOptions, string?> listener) => null;
    }

    // Every role that holds a right is configured (has an office level), as the DB snapshot guarantees (D2).
    private static BudgetRightsOptions Rights(params (string Key, string[] Roles)[] rights) => new()
    {
        Rights = rights.ToDictionary(r => r.Key, r => r.Roles),
        RoleOfficeLevels = rights.SelectMany(r => r.Roles).Distinct(StringComparer.OrdinalIgnoreCase)
            .ToDictionary(r => r, _ => "DIVISION", StringComparer.OrdinalIgnoreCase),
    };

    private static readonly BudgetRightsOptions Default = Rights(
        ("F07.Submit", ["DDO"]),
        ("Budget.View", ["DDO", "FIN_VIEW"]));

    private static ClaimsPrincipal User(params string[] budgetRoles)
    {
        var project = JsonSerializer.Serialize(new
        {
            projectId = Guid.NewGuid(),
            projectName = "Budget",
            roles = budgetRoles.Select(r => new { roleId = r, roleName = r }),
        });
        var other = JsonSerializer.Serialize(new
        {
            projectId = Guid.NewGuid(),
            projectName = "Payroll",
            roles = new[] { new { roleId = "DDO", roleName = "DDO" } },
        });
        return new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("projects", project), new Claim("projects", other)], "Bearer"));
    }

    private static async Task<(bool Succeeded, HttpContext Http)> Evaluate(
        IOptionsMonitor<BudgetRightsOptions> monitor, ClaimsPrincipal user, string key,
        string? header = null, HttpContext? http = null)
    {
        http ??= new DefaultHttpContext();
        if (header is not null) http.Request.Headers[BudgetActingRole.HeaderName] = header;
        var handler = new BudgetRightHandler(monitor, new HttpContextAccessor { HttpContext = http },
            NullLogger<BudgetRightHandler>.Instance);
        var requirement = new BudgetRightRequirement(key);
        var ctx = new AuthorizationHandlerContext([requirement], user, null);
        await handler.HandleAsync(ctx);
        return (ctx.HasSucceeded, http);
    }

    [Fact]
    public async Task BUD030_single_role_no_header_succeeds_when_role_holds_right()
    {
        var (ok, http) = await Evaluate(new MutableMonitor(Default), User("DDO"), "F07.Submit");
        ok.Should().BeTrue();
        BudgetActingRole.Get(http).Should().Be("DDO");
    }

    [Fact]
    public async Task BUD030_single_role_without_the_right_is_refused()
    {
        var (ok, _) = await Evaluate(new MutableMonitor(Default), User("FIN_VIEW"), "F07.Submit");
        ok.Should().BeFalse();
    }

    [Fact]
    public async Task BUD033_DdoPlusFinViewActingAsFinViewRefusedF07Submit()
    {
        var (ok, http) = await Evaluate(new MutableMonitor(Default), User("DDO", "FIN_VIEW"), "F07.Submit", "FIN_VIEW");
        ok.Should().BeFalse("the right is evaluated against the acting role only");
        BudgetActingRole.Get(http).Should().Be("FIN_VIEW");
    }

    [Fact]
    public async Task BUD033_DdoPlusFinViewActingAsDdoSucceedsF07Submit()
    {
        var (ok, _) = await Evaluate(new MutableMonitor(Default), User("DDO", "FIN_VIEW"), "F07.Submit", "ddo");
        ok.Should().BeTrue();
    }

    [Fact]
    public async Task BUD030_header_naming_a_role_not_held_is_refused()
    {
        var (ok, http) = await Evaluate(new MutableMonitor(Default), User("FIN_VIEW"), "Budget.View", "DDO");
        ok.Should().BeFalse();
        BudgetActingRole.Get(http).Should().BeNull();
    }

    [Fact]
    public async Task BUD030_two_roles_no_header_is_refused()
    {
        var (ok, _) = await Evaluate(new MutableMonitor(Default), User("DDO", "FIN_VIEW"), "Budget.View");
        ok.Should().BeFalse();
    }

    [Fact]
    public async Task BUD030_role_held_only_in_another_project_is_refused()
    {
        var (ok, _) = await Evaluate(new MutableMonitor(Default), User(), "F07.Submit");
        ok.Should().BeFalse("DDO is held in Payroll, not in Budget");
    }

    [Fact]
    public async Task BUD033_options_changed_between_evaluations_change_the_decision()
    {
        var monitor = new MutableMonitor(Default);
        (await Evaluate(monitor, User("DDO"), "F07.Submit")).Succeeded.Should().BeTrue();

        monitor.CurrentValue = Rights(("F07.Submit", ["SE"]));
        (await Evaluate(monitor, User("DDO"), "F07.Submit")).Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task BUD033_unknown_key_fails_closed()
    {
        var (ok, _) = await Evaluate(new MutableMonitor(Default), User("DDO"), "No.Such.Right");
        ok.Should().BeFalse();
    }

    [Fact]
    public async Task Second_evaluation_in_the_request_reuses_the_acting_role()
    {
        var monitor = new MutableMonitor(Default);
        var http = new DefaultHttpContext();
        http.Request.Headers[BudgetActingRole.HeaderName] = "FIN_VIEW";
        (await Evaluate(monitor, User("DDO", "FIN_VIEW"), "Budget.View", http: http)).Succeeded.Should().BeTrue();

        // Header removed mid-request: the stored acting role still governs.
        http.Request.Headers.Remove(BudgetActingRole.HeaderName);
        (await Evaluate(monitor, User("DDO", "FIN_VIEW"), "F07.Submit", http: http)).Succeeded.Should().BeFalse();
    }
}
