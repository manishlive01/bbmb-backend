using System.Security.Claims;
using BBMB.BudgetService.Features.EmployeeCharges.Entities;
using BBMB.BudgetService.Features.Employees.Entities;
using BBMB.BudgetService.Shared.ServiceDTOs;
using BBMB.BudgetService.Shared.Services.EmployeeChargeServices;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BBMB.BudgetService.Tests.Shared.Services;

/// <summary>Design §4.3 "Charge selection", §3 D-12, §14 (charge selection).</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class ChargeResolverTests(BudgetDatabaseFixture fixture)
{
    private const string BudgetProject = "BU";

    private async Task<(Guid UserId, Guid EmployeeId)> NewEmployeeAsync()
    {
        await using var emp = fixture.NewEmployeeContext();
        var template = await emp.Employees.AsNoTracking().FirstAsync(e => e.Id == fixture.MigrationEmployeeId);
        var e = new Employee
        {
            Id = Guid.NewGuid(),
            EmployeeCode = Random.Shared.Next(100000, 899999),
            UserId = Guid.NewGuid(),
            TitleId = template.TitleId,
            GenderId = template.GenderId,
            NameEnglish = "Charge Test",
            DateOfBirth = template.DateOfBirth,
            PhoneNumber = "0",
            Email = "x@example.invalid",
            Address = "x",
        };
        emp.Employees.Add(e);
        await emp.SaveChangesAsync();
        return (e.UserId, e.Id);
    }

    private async Task<Guid> AddChargeAsync(Guid employeeId, string project = BudgetProject,
        DateTime? expireTime = null, bool isActive = true, bool isExpire = false)
    {
        await using var emp = fixture.NewEmployeeContext();
        var c = new EmployeeCharge
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            ProjectCode = project,
            CEId = 1,
            DivisionId = 1,
            ChargeTime = DateTime.UtcNow.AddDays(-2),
            ChargeExpireTime = expireTime,
            IsActive = isActive,
            IsChargeExpire = isExpire,
        };
        emp.EmployeeCharges.Add(c);
        await emp.SaveChangesAsync();
        return c.Id;
    }

    private ChargeResolver Resolver(Guid userId, string? chargeHeader = null, string? projectCode = BudgetProject)
    {
        var http = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", userId.ToString())], "Bearer")),
        };
        if (chargeHeader is not null) http.Request.Headers[BudgetChargeResult.HeaderName] = chargeHeader;
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { [ChargeResolver.ProjectCodeKey] = projectCode })
            .Build();
        return new ChargeResolver(new HttpContextAccessor { HttpContext = http }, fixture.NewEmployeeContext(), config);
    }

    [Fact]
    public async Task D12_zero_charges_is_None()
    {
        var (user, _) = await NewEmployeeAsync();
        (await Resolver(user).GetCurrentBudgetChargeAsync(default)).Outcome.Should().Be(BudgetChargeOutcome.None);
    }

    [Fact]
    public async Task D12_one_charge_is_Single_with_its_id()
    {
        var (user, emp) = await NewEmployeeAsync();
        var id = await AddChargeAsync(emp);
        var r = await Resolver(user).GetCurrentBudgetChargeAsync(default);
        r.Outcome.Should().Be(BudgetChargeOutcome.Single);
        r.Charge!.ChargeId.Should().Be(id);
        r.Charge.EmployeeId.Should().Be(emp);
    }

    [Fact]
    public async Task D12_two_charges_no_header_is_Ambiguous_listing_both()
    {
        var (user, emp) = await NewEmployeeAsync();
        var a = await AddChargeAsync(emp);
        var b = await AddChargeAsync(emp);
        var r = await Resolver(user).GetCurrentBudgetChargeAsync(default);
        r.Outcome.Should().Be(BudgetChargeOutcome.Ambiguous);
        r.Charge.Should().BeNull("the resolver never picks one");
        r.ChargeIds.Should().BeEquivalentTo([a, b]);
    }

    [Fact]
    public async Task D12_header_naming_one_of_two_selects_it()
    {
        var (user, emp) = await NewEmployeeAsync();
        await AddChargeAsync(emp);
        var b = await AddChargeAsync(emp);
        var r = await Resolver(user, b.ToString()).GetCurrentBudgetChargeAsync(default);
        r.Outcome.Should().Be(BudgetChargeOutcome.Single);
        r.Charge!.ChargeId.Should().Be(b);
    }

    [Fact]
    public async Task D12_header_naming_a_charge_not_held_is_None()
    {
        var (user, emp) = await NewEmployeeAsync();
        await AddChargeAsync(emp);
        await AddChargeAsync(emp);
        (await Resolver(user, Guid.NewGuid().ToString()).GetCurrentBudgetChargeAsync(default))
            .Outcome.Should().Be(BudgetChargeOutcome.None);
        (await Resolver(user, fixture.MigrationChargeId.ToString()).GetCurrentBudgetChargeAsync(default))
            .Outcome.Should().Be(BudgetChargeOutcome.None, "another employee's charge is not held");
        (await Resolver(user, "not-a-guid").GetCurrentBudgetChargeAsync(default))
            .Outcome.Should().Be(BudgetChargeOutcome.None);
    }

    [Fact]
    public async Task D12_another_projects_charge_is_ignored()
    {
        var (user, emp) = await NewEmployeeAsync();
        await AddChargeAsync(emp, project: "PY");
        var budget = await AddChargeAsync(emp);
        var r = await Resolver(user).GetCurrentBudgetChargeAsync(default);
        r.Outcome.Should().Be(BudgetChargeOutcome.Single);
        r.Charge!.ChargeId.Should().Be(budget);
    }

    [Fact]
    public async Task D12_unset_project_code_fails_closed()
    {
        var (user, emp) = await NewEmployeeAsync();
        await AddChargeAsync(emp);
        (await Resolver(user, projectCode: null).GetCurrentBudgetChargeAsync(default))
            .Outcome.Should().Be(BudgetChargeOutcome.None);
    }

    [Fact]
    public async Task D12_charge_past_expire_time_but_not_flagged_is_not_returned_by_either_method()
    {
        var (user, emp) = await NewEmployeeAsync();
        await AddChargeAsync(emp, expireTime: DateTime.UtcNow.AddMinutes(-1), isExpire: false);

        (await Resolver(user).GetCurrentBudgetChargeAsync(default)).Outcome.Should().Be(BudgetChargeOutcome.None);
        (await Resolver(user).GetCurrentChargeAsync(default)).Should().BeNull();
    }

    [Fact]
    public async Task D12_charge_with_future_expire_time_is_current_and_carries_ChargeId()
    {
        var (user, emp) = await NewEmployeeAsync();
        var id = await AddChargeAsync(emp, expireTime: DateTime.UtcNow.AddDays(1));
        (await Resolver(user).GetCurrentChargeAsync(default))!.ChargeId.Should().Be(id);
        (await Resolver(user).GetCurrentBudgetChargeAsync(default)).Charge!.ChargeId.Should().Be(id);
    }
}
