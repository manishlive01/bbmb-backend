using Auth.Authorization.Requirements;
using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Shared.ServiceDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BBMB.BudgetService.Tests.Shared.Services;

/// <summary>Design §4.4 "Office", BUD-031.</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class BudgetOfficeScopeTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private string _cs = string.Empty;
    private Offices _o = default!;

    private sealed record Offices(
        long Finance, long Secretariat, long CeA, long CircleA, long DivisionA, long CeB, long CircleB, long DivisionB,
        TestMasters.Hierarchy HierarchyA, TestMasters.Hierarchy HierarchyB);

    private static IOptionsMonitor<BudgetRightsOptions> Options() => new StaticMonitor(new BudgetRightsOptions
    {
        RoleOfficeLevels = new(StringComparer.OrdinalIgnoreCase)
        {
            ["DDO"] = "DIVISION", ["SE"] = "CIRCLE", ["CE"] = "CE", ["SECRETARY"] = "SECRETARIAT",
            ["AFA_BUDGET"] = "FINANCE", ["FIN_VIEW"] = "FINANCE", ["IT_CELL"] = "IT_CELL",
        },
    });

    private sealed class StaticMonitor(BudgetRightsOptions value) : IOptionsMonitor<BudgetRightsOptions>
    {
        public BudgetRightsOptions CurrentValue => value;
        public BudgetRightsOptions Get(string? name) => value;
        public IDisposable? OnChange(Action<BudgetRightsOptions, string?> listener) => null;
    }

    public async Task InitializeAsync()
    {
        // Own database: the T-13 tree below is committed.
        _cs = await fixture.CreateFreshDatabaseAsync($"office_scope_{Guid.NewGuid():N}"[..24]);
        await fixture.MigrateToAsync(_cs, null);
        var a = await TestMasters.InsertDivisionAsync(_cs);
        var b = await TestMasters.InsertDivisionAsync(_cs);
        var source = await TestActor.CreateAsync(fixture, connectionString: _cs);

        await using var ctx = BudgetDatabaseFixture.NewBudgetContext(_cs);
        await ctx.Database.ExecuteSqlRawAsync(
            "INSERT INTO budget.irr_pwr (irr_pwr_code, irr_pwr_desc, modification_date, user_code) " +
            "VALUES ('I', 'Irrigation', now(), 'TEST') ON CONFLICT DO NOTHING");
        await using var tx = await new BudgetActionScope(ctx, source).BeginAsync("TEST_OFFICES");

        async Task<long> Add(BudgetOffice o) { ctx.BudgetOffices.Add(o); await ctx.SaveChangesAsync(); return o.OfficeId; }
        var finance = await Add(new BudgetOffice { OfficeLevel = "FINANCE", OfficeName = "Finance" });
        var secretariat = await Add(new BudgetOffice { OfficeLevel = "SECRETARIAT", OfficeName = "Secretariat", ParentOfficeId = finance });
        var ceA = await Add(new BudgetOffice { OfficeLevel = "CE", CeId = a.CeId, WingCode = "I", ParentOfficeId = secretariat });
        var crA = await Add(new BudgetOffice { OfficeLevel = "CIRCLE", CircleId = a.CrId, ParentOfficeId = ceA });
        var dvA = await Add(new BudgetOffice { OfficeLevel = "DIVISION", DivisionId = a.DivisionId, ParentOfficeId = crA });
        var ceB = await Add(new BudgetOffice { OfficeLevel = "CE", CeId = b.CeId, WingCode = "I", ParentOfficeId = secretariat });
        var crB = await Add(new BudgetOffice { OfficeLevel = "CIRCLE", CircleId = b.CrId, ParentOfficeId = ceB });
        var dvB = await Add(new BudgetOffice { OfficeLevel = "DIVISION", DivisionId = b.DivisionId, ParentOfficeId = crB });
        await tx.CommitAsync();
        _o = new Offices(finance, secretariat, ceA, crA, dvA, ceB, crB, dvB, a, b);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private BudgetOfficeScope Scope(Microsoft.EntityFrameworkCore.DbContext? _ = null) =>
        new(BudgetDatabaseFixture.NewBudgetContext(_cs), Options());

    private UserChargeContext Charge(int? division = null, int? cr = null, int? ce = null) =>
        new() { DivisionId = division, CRId = cr, CEId = ce, ProjectCode = "BU" };

    [Fact]
    public async Task BUD031_DDO_SE_CE_resolve_by_division_circle_and_ce_ids()
    {
        var s = Scope();
        (await s.ResolveActorOfficeAsync(Charge(division: _o.HierarchyA.DivisionId), "DDO"))!.OfficeId.Should().Be(_o.DivisionA);
        (await s.ResolveActorOfficeAsync(Charge(cr: _o.HierarchyA.CrId), "SE"))!.OfficeId.Should().Be(_o.CircleA);
        (await s.ResolveActorOfficeAsync(Charge(ce: _o.HierarchyB.CeId), "ce"))!.OfficeId.Should().Be(_o.CeB);
    }

    [Fact]
    public async Task BUD031_Finance_roles_resolve_to_FINANCE_and_Secretary_to_SECRETARIAT()
    {
        var s = Scope();
        (await s.ResolveActorOfficeAsync(Charge(), "AFA_BUDGET"))!.Should().Be(new ActorOffice(_o.Finance, "FINANCE"));
        (await s.ResolveActorOfficeAsync(Charge(), "FIN_VIEW"))!.OfficeId.Should().Be(_o.Finance);
        (await s.ResolveActorOfficeAsync(Charge(), "SECRETARY"))!.OfficeId.Should().Be(_o.Secretariat);
        (await s.ResolveActorOfficeAsync(Charge(), "IT_CELL"))!.OfficeLevel.Should().Be("IT_CELL");
    }

    [Fact]
    public async Task M20_no_matching_office_resolves_to_null()
    {
        var s = Scope();
        (await s.ResolveActorOfficeAsync(Charge(division: -1), "DDO")).Should().BeNull();
        (await s.ResolveActorOfficeAsync(Charge(), "DDO")).Should().BeNull("the charge carries no division");
        (await s.ResolveActorOfficeAsync(Charge(division: _o.HierarchyA.DivisionId), "UNKNOWN_ROLE")).Should().BeNull();
    }

    [Fact]
    public async Task BUD031_visible_set_of_a_CE_contains_its_circles_and_divisions_only()
    {
        var visible = await Scope().GetVisibleOfficeIdsAsync(_o.CeA);
        visible.Should().BeEquivalentTo([_o.CeA, _o.CircleA, _o.DivisionA]);
        visible.Should().NotContain([_o.CeB, _o.CircleB, _o.DivisionB, _o.Secretariat]);

        (await Scope().GetVisibleOfficeIdsAsync(_o.DivisionB)).Should().BeEquivalentTo([_o.DivisionB]);
    }

    [Fact]
    public async Task BUD031_Finance_and_Secretariat_see_every_office()
    {
        await using var ctx = BudgetDatabaseFixture.NewBudgetContext(_cs);
        var all = await ctx.BudgetOffices.Select(o => o.OfficeId).ToListAsync();
        (await Scope().GetVisibleOfficeIdsAsync(_o.Finance)).Should().BeEquivalentTo(all);
        (await Scope().GetVisibleOfficeIdsAsync(_o.Secretariat)).Should().BeEquivalentTo(all);
    }
}
