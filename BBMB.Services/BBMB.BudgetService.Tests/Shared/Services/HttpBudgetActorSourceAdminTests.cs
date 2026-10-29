using System.Security.Claims;
using BBMB.BudgetService.Authorization.Handlers;
using BBMB.BudgetService.Features.BudgetProcess.AuthzConfig;
using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.EmployeeCharges.Entities;
using BBMB.BudgetService.Features.Employees.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using BBMB.BudgetService.Shared.ServiceDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using BBMB.BudgetService.Shared.Services.EmployeeChargeServices;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace BBMB.BudgetService.Tests.Shared.Services;

/// <summary>
/// The effective-admin branch of <see cref="HttpBudgetActorSource"/>: an admin acting role resolves to a
/// valid actor without an office-level-matched operational charge, while a non-admin role keeps today's
/// charge-driven behaviour exactly. In-memory, no PostgreSQL (the DB forbid/require triggers are exercised
/// by the Testcontainers suites, which Docker cannot run here).
/// </summary>
public sealed class HttpBudgetActorSourceAdminTests
{
    private const string AdminRole = "ProjectAdmin";
    private const string BudgetProject = "BU";
    private const long FinanceOfficeId = 500L;

    private static EmployeeContext NewEmployeeContext() =>
        new(new DbContextOptionsBuilder<EmployeeContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static HttpContextAccessor Http(Guid userId, string role)
    {
        var http = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", userId.ToString())], "Bearer")),
        };
        http.Items[BudgetActingRole.ItemKey] = role;
        return new HttpContextAccessor { HttpContext = http };
    }

    private static HttpBudgetActorSource Source(
        HttpContextAccessor http, EmployeeContext emp, IChargeResolver charge, IBudgetOfficeScope office) =>
        new(http, charge, office, new FakeAdminConfigProvider(), emp, NullLogger<HttpBudgetActorSource>.Instance);

    private static async Task<(Guid UserId, Guid EmployeeId)> SeedEmployeeAsync(EmployeeContext emp)
    {
        var e = new Employee
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            NameEnglish = "Admin User",
            PhoneNumber = "0",
            Email = "x@example.invalid",
            Address = "x",
        };
        emp.Employees.Add(e);
        await emp.SaveChangesAsync();
        return (e.UserId, e.Id);
    }

    private static async Task<Guid> SeedChargeAsync(
        EmployeeContext emp, Guid employeeId, string project = BudgetProject, bool active = true)
    {
        var c = new EmployeeCharge
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            ProjectCode = project,
            CEId = 1,
            ChargeTime = DateTime.UtcNow.AddDays(-1),
            IsActive = active,
            IsChargeExpire = false,
        };
        emp.EmployeeCharges.Add(c);
        await emp.SaveChangesAsync();
        return c.Id;
    }

    [Fact]
    public async Task Admin_with_a_budget_charge_resolves_to_the_admin_office_and_stamps_that_charge()
    {
        await using var emp = NewEmployeeContext();
        var (userId, employeeId) = await SeedEmployeeAsync(emp);
        var chargeId = await SeedChargeAsync(emp, employeeId);

        var source = Source(Http(userId, AdminRole), emp,
            new ThrowingChargeResolver(), new FinanceOnlyOfficeScope());

        var resolution = await source.ResolveAsync();

        resolution.Actor.Should().NotBeNull("an effective admin resolves without an operational charge match");
        resolution.Actor!.RoleCode.Should().Be(AdminRole);
        resolution.Actor.OfficeId.Should().Be(FinanceOfficeId);
        resolution.Actor.OfficeLevel.Should().Be(OfficeLevel.Finance);
        resolution.Actor.EmployeeId.Should().Be(employeeId);
        resolution.Actor.ChargeId.Should().Be(chargeId, "the budget charge stamps the audit actor");
        resolution.Actor.ActorName.Should().Be("Admin User");
    }

    [Fact]
    public async Task Admin_with_no_charge_at_all_fails_with_NO_ADMIN_CHARGE_not_M20()
    {
        await using var emp = NewEmployeeContext();
        var (userId, _) = await SeedEmployeeAsync(emp);

        var source = Source(Http(userId, AdminRole), emp,
            new ThrowingChargeResolver(), new FinanceOnlyOfficeScope());

        var resolution = await source.ResolveAsync();

        resolution.Actor.Should().BeNull();
        resolution.Reason.Should().Be("NO_ADMIN_CHARGE",
            "the audit trigger requires a real charge; the operator must attach one");
    }

    [Fact]
    public async Task Admin_with_no_office_of_its_level_fails_with_NO_ADMIN_OFFICE()
    {
        await using var emp = NewEmployeeContext();
        var (userId, employeeId) = await SeedEmployeeAsync(emp);
        await SeedChargeAsync(emp, employeeId);

        var source = Source(Http(userId, AdminRole), emp,
            new ThrowingChargeResolver(), new NoOfficeScope());

        var resolution = await source.ResolveAsync();

        resolution.Actor.Should().BeNull();
        resolution.Reason.Should().Be("NO_ADMIN_OFFICE");
    }

    [Fact]
    public async Task Non_admin_role_still_resolves_through_the_charge_and_keeps_NO_CHARGE()
    {
        await using var emp = NewEmployeeContext();
        var (userId, _) = await SeedEmployeeAsync(emp);

        // A non-admin role never enters the admin branch: it uses the charge resolver, which returns None here.
        var source = Source(Http(userId, "DDO"), emp,
            new FixedChargeResolver(BudgetChargeResult.None), new FinanceOnlyOfficeScope());

        var resolution = await source.ResolveAsync();

        resolution.Actor.Should().BeNull();
        resolution.Reason.Should().Be("NO_CHARGE", "non-admin behaviour is unchanged");
    }

    // --- Stubs ------------------------------------------------------------------------------------

    /// <summary>Marks only <see cref="AdminRole"/> as an effective admin, with the Budget project code.</summary>
    private sealed class FakeAdminConfigProvider : IBudgetConfigProvider
    {
        public Task<BudgetConfigSnapshot> GetAsync(CancellationToken ct = default) =>
            Task.FromResult(new BudgetConfigSnapshot { ProjectCode = BudgetProject, AdminRoles = [AdminRole] });
        public void Invalidate() { }
    }

    /// <summary>FINANCE resolves to a single office; any other level resolves to nothing.</summary>
    private sealed class FinanceOnlyOfficeScope : IBudgetOfficeScope
    {
        public Task<ActorOffice?> ResolveActorOfficeAsync(UserChargeContext charge, string role, CancellationToken ct = default) =>
            Task.FromResult<ActorOffice?>(new ActorOffice(FinanceOfficeId, OfficeLevel.Finance));
        public Task<IReadOnlySet<long>> GetVisibleOfficeIdsAsync(long officeId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlySet<long>>(new HashSet<long>());
    }

    /// <summary>No office of the role's level exists.</summary>
    private sealed class NoOfficeScope : IBudgetOfficeScope
    {
        public Task<ActorOffice?> ResolveActorOfficeAsync(UserChargeContext charge, string role, CancellationToken ct = default) =>
            Task.FromResult<ActorOffice?>(null);
        public Task<IReadOnlySet<long>> GetVisibleOfficeIdsAsync(long officeId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlySet<long>>(new HashSet<long>());
    }

    /// <summary>The admin branch must never call the charge resolver; a call is a defect.</summary>
    private sealed class ThrowingChargeResolver : IChargeResolver
    {
        public Task<UserChargeContext?> GetCurrentChargeAsync(CancellationToken ct) =>
            throw new InvalidOperationException("the admin branch must not resolve an operational charge");
        public Task<BudgetChargeResult> GetCurrentBudgetChargeAsync(CancellationToken ct) =>
            throw new InvalidOperationException("the admin branch must not resolve an operational charge");
    }

    private sealed class FixedChargeResolver(BudgetChargeResult result) : IChargeResolver
    {
        public Task<UserChargeContext?> GetCurrentChargeAsync(CancellationToken ct) =>
            Task.FromResult<UserChargeContext?>(null);
        public Task<BudgetChargeResult> GetCurrentBudgetChargeAsync(CancellationToken ct) =>
            Task.FromResult(result);
    }
}
