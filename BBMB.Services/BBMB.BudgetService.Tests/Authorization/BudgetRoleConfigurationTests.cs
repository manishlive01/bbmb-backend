using System.Security.Claims;
using System.Text.Json;
using BBMB.BudgetService.Authorization.Handlers;
using BBMB.BudgetService.Authorization.Requirements;
using BBMB.BudgetService.Features.AdminConfig;
using BBMB.BudgetService.Features.BudgetProcess.AuthzConfig;
using BBMB.BudgetService.Features.BudgetProcess.AuthzConfig.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.HierarchyManagement.LevelManagement.Entites;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ServiceDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using BBMB.BudgetService.Tests.Common;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
namespace BBMB.BudgetService.Tests.Authorization;
/// <summary>
/// Dynamic role configuration (D1, D2) in memory: the snapshot built from the tables by the production
/// <see cref="BudgetConfigProvider.BuildSnapshotAsync"/>, read through the production options adapter by
/// <see cref="BudgetOfficeScope"/> and <see cref="BudgetRightHandler"/>, and the <c>roles/configure</c> handler.
/// Each test seeds exactly the rows it needs.
/// </summary>
public sealed class BudgetRoleConfigurationTests : InMemoryTestBase
{
    private const int DivisionLevel = 1, FinanceLevel = 5, SubDivisionLevel = 9, RetiredLevel = 10;
    private readonly FakeBudgetActionScope _scope = new();

    public BudgetRoleConfigurationTests()
    {
        Context.BudgetProjectSettings.Add(new BudgetProjectSettings
        {
            SsoProjectName = "Budget", EmployeeChargeProjectCode = "1", EditLeaseMinutes = 20,
        });
        Context.Levels.AddRange(
            new Levels { Id = DivisionLevel, Name = "Division", Status = true },
            new Levels { Id = FinanceLevel, Name = " finance ", Status = true },
            new Levels { Id = SubDivisionLevel, Name = "Sub Division", Status = true },
            new Levels { Id = RetiredLevel, Name = "CIRCLE", Status = false });
        Context.BudgetRights.AddRange(
            new BudgetRight { RightKey = "Budget.View" },
            new BudgetRight { RightKey = "F07.Submit" },
            new BudgetRight { RightKey = "Settings.Admin" });
        Context.BudgetQueryFields.AddRange(new BudgetQueryField { FieldName = "category" }, new BudgetQueryField { FieldName = "wing" });
        Context.SaveChanges();
    }

    // ---- seeding helpers: explicit rows, no builders ----
    private BudgetRole AddRole(string name, int? levelId, bool active = true, bool ssoActive = true, params string[] rights)
    {
        var role = new BudgetRole { RoleName = name, OfficeLevelId = levelId, IsActive = active, SsoIsActive = ssoActive };
        Context.BudgetRoles.Add(role);
        Context.SaveChanges();
        foreach (var key in rights)
            Context.BudgetRoleRights.Add(new BudgetRoleRight { RoleId = role.Id, RightId = Context.BudgetRights.Single(r => r.RightKey == key).Id });
        Context.SaveChanges();
        return role;
    }

    private void Grant(BudgetRole role, string field, bool active = true)
    {
        Context.BudgetRoleQueryFields.Add(new BudgetRoleQueryField
        {
            RoleId = role.Id, QueryFieldId = Context.BudgetQueryFields.Single(f => f.FieldName == field).Id, IsActive = active,
        });
        Context.SaveChanges();
    }

    private Task<BudgetConfigSnapshot> SnapshotAsync() =>
        new BudgetConfigProvider(Context, new BudgetConfigCache(), NullLogger<BudgetConfigProvider>.Instance).BuildSnapshotAsync();

    /// <summary>The production options adapter over a pre-filled cache (no DB read on CurrentValue).</summary>
    private static BudgetConfigOptionsMonitor Monitor(BudgetConfigSnapshot snapshot)
    {
        var cache = new BudgetConfigCache();
        cache.Store(snapshot, 1, DateTime.UtcNow);
        return new BudgetConfigOptionsMonitor(scopeFactory: null!, cache);
    }

    private static FakeBudgetActorSource Actor(string role) => new(new BudgetActor(Guid.NewGuid(), Guid.NewGuid(), 1,
        OfficeLevel.ItCell, role, "Admin", null, FakeBudgetActorSource.TestIp, Guid.NewGuid()));

    private sealed class FixedConfig(BudgetConfigSnapshot snapshot) : IBudgetConfigProvider
    {
        public int Invalidations { get; private set; }
        public Task<BudgetConfigSnapshot> GetAsync(CancellationToken ct = default) => Task.FromResult(snapshot);
        public void Invalidate() => Invalidations++;
    }

    private async Task<BBMB.BudgetService.Shared.ResponseDTOs.Response<BudgetRoleConfigDto>> ConfigureAsync(
        long roleId, int levelId, string[] rights, string[] fields, string actingRole = "IT_CELL", FixedConfig? config = null)
    {
        var handler = new ConfigureRoleHandler(Context, Actor(actingRole), _scope, config ?? new FixedConfig(BudgetConfigSnapshot.Empty));
        var r = await handler.Handle(new ConfigureRoleCommand(new ConfigureRoleDto(roleId, levelId, rights, fields)), default);
        Context.ChangeTracker.Clear();
        return r;
    }

    // ---------------- denial of unconfigured / inactive roles (D2) ----------------

    [Fact]
    public async Task An_unconfigured_role_gets_no_rights_no_office_and_no_query_field()
    {
        var noLevel = AddRole("NEW_SYNCED", levelId: null, rights: "Budget.View");
        AddRole("NO_RIGHTS", DivisionLevel);
        Grant(noLevel, "category");

        var s = await SnapshotAsync();

        s.RoleOfficeLevels.Should().BeEmpty();
        s.Rights["Budget.View"].Should().BeEmpty();
        s.QueryFieldRoles["category"].Should().BeEmpty();
        var office = await new BudgetOfficeScope(Context, Monitor(s))
            .ResolveActorOfficeAsync(new UserChargeContext(), "NEW_SYNCED");
        office.Should().BeNull();
    }

    [Fact]
    public async Task An_inactive_or_SSO_inactive_role_is_denied()
    {
        AddRole("OFF_IN_BUDGET", DivisionLevel, active: false, rights: "Budget.View");
        AddRole("OFF_IN_SSO", DivisionLevel, ssoActive: false, rights: "Budget.View");
        AddRole("DDO", DivisionLevel, rights: "Budget.View");

        var s = await SnapshotAsync();

        s.RoleOfficeLevels.Keys.Should().Equal("DDO");
        s.Rights["Budget.View"].Should().Equal("DDO");
    }

    [Fact]
    public async Task An_unconfigured_admin_role_is_not_an_effective_admin_even_when_approved()
    {
        var admin = AddRole("ProjectAdmin", levelId: null, rights: "Settings.Admin");
        admin.IsAdmin = true;
        Context.BudgetAdminRoleAssignments.Add(new BudgetAdminRoleAssignment
        {
            RoleId = admin.Id, Status = AdminAssignmentStatus.Approved, DocumentRef = "DOC-1",
            UploadedBy = Guid.NewGuid(), ApprovedBy = Guid.NewGuid(),
        });
        await Context.SaveChangesAsync();

        (await SnapshotAsync()).AdminRoles.Should().BeEmpty();
    }

    // ---------------- office level from the FK (D1) ----------------

    [Fact]
    public async Task The_office_level_is_resolved_from_the_levels_row_and_drives_the_office_scope()
    {
        AddRole("AFA_BUDGET", FinanceLevel, rights: "Budget.View");
        AddRole("ON_SUB_DIVISION", SubDivisionLevel, rights: "Budget.View");
        AddRole("ON_RETIRED_LEVEL", RetiredLevel, rights: "Budget.View");
        Context.BudgetOffices.Add(new BudgetOffice { OfficeId = 500, OfficeLevel = OfficeLevel.Finance, OfficeName = "Finance", IsActive = true });
        await Context.SaveChangesAsync();

        var s = await SnapshotAsync();

        s.RoleOfficeLevels.Should().ContainKey("AFA_BUDGET").WhoseValue.Should().Be(OfficeLevel.Finance,
            "' finance ' matches FINANCE trimmed and case-insensitively");
        s.RoleOfficeLevels.Should().NotContainKey("ON_SUB_DIVISION", "a level that is not a Budget office level is unconfigured");
        s.RoleOfficeLevels.Should().ContainKey("ON_RETIRED_LEVEL").WhoseValue.Should().Be(OfficeLevel.Circle,
            "levels.status belongs to the hierarchy master and is ignored by Budget authorisation");
        var office = await new BudgetOfficeScope(Context, Monitor(s)).ResolveActorOfficeAsync(new UserChargeContext(), "AFA_BUDGET");
        office.Should().Be(new ActorOffice(500, OfficeLevel.Finance));
    }

    [Fact]
    public async Task A_configured_role_gets_exactly_its_active_query_fields()
    {
        var ddo = AddRole("DDO", DivisionLevel, rights: "Budget.View");
        Grant(ddo, "category");
        Grant(ddo, "wing", active: false);

        var s = await SnapshotAsync();

        s.QueryFieldRoles["category"].Should().Equal("DDO");
        s.QueryFieldRoles["wing"].Should().BeEmpty("a soft-revoked grant is not read");
    }

    // ---------------- acting-role selection ----------------

    private static ClaimsPrincipal User(params string[] roles) => new(new ClaimsIdentity(
    [
        new Claim("projects", JsonSerializer.Serialize(new
        {
            projectId = Guid.NewGuid(), projectName = "Budget",
            roles = roles.Select(r => new { roleId = r, roleName = r }),
        })),
    ], "Bearer"));

    private static async Task<(bool Ok, HttpContext Http)> AuthorizeAsync(BudgetConfigSnapshot s, ClaimsPrincipal user, string right)
    {
        var http = new DefaultHttpContext();
        var handler = new BudgetRightHandler(Monitor(s), new HttpContextAccessor { HttpContext = http },
            NullLogger<BudgetRightHandler>.Instance);
        var requirement = new BudgetRightRequirement(right);
        var ctx = new AuthorizationHandlerContext([requirement], user, null);
        await handler.HandleAsync(ctx);
        return (ctx.HasSucceeded, http);
    }

    [Fact]
    public async Task Acting_role_selection_ignores_an_unconfigured_role()
    {
        AddRole("DDO", DivisionLevel, rights: "F07.Submit");
        AddRole("NEW_SYNCED", levelId: null);
        var s = await SnapshotAsync() with { SsoProjectName = "Budget" };

        // Two Budget roles held, but only one is configured: it is the single candidate, no header needed.
        var (ok, http) = await AuthorizeAsync(s, User("DDO", "NEW_SYNCED"), "F07.Submit");
        ok.Should().BeTrue();
        BudgetActingRole.Get(http).Should().Be("DDO");
        BudgetActingRole.GetSelection(http).Should().BeNull("no selection prompt lists an unconfigured role");

        var (onlyUnconfigured, http2) = await AuthorizeAsync(s, User("NEW_SYNCED"), "F07.Submit");
        onlyUnconfigured.Should().BeFalse();
        BudgetActingRole.Get(http2).Should().BeNull();
    }

    // ---------------- roles/configure (replace-set) ----------------

    [Fact]
    public async Task Configure_grants_reactivates_and_deactivates_to_match_the_request()
    {
        var role = AddRole("SE", levelId: null, rights: "Budget.View");
        var f07 = Context.BudgetRights.Single(r => r.RightKey == "F07.Submit").Id;
        Context.BudgetRoleRights.Add(new BudgetRoleRight { RoleId = role.Id, RightId = f07, IsActive = false });
        Grant(role, "wing");
        await Context.SaveChangesAsync();

        var r = await ConfigureAsync(role.Id, DivisionLevel, ["F07.Submit", "Settings.Admin"], ["category"]);

        r.IsSucced.Should().BeTrue(r.Message);
        r.Data!.OfficeLevelId.Should().Be(DivisionLevel);
        r.Data.OfficeLevelName.Should().Be("Division");
        r.Data.IsConfigured.Should().BeTrue();
        r.Data.RightKeys.Should().Equal("F07.Submit", "Settings.Admin");
        r.Data.QueryFieldNames.Should().Equal("category");
        var grants = Context.BudgetRoleRights.AsNoTracking().Where(x => x.RoleId == role.Id)
            .Join(Context.BudgetRights, x => x.RightId, k => k.Id, (x, k) => new { k.RightKey, x.IsActive }).ToList();
        grants.Should().BeEquivalentTo(new[]
        {
            new { RightKey = "Budget.View", IsActive = false },    // not listed: soft-deactivated
            new { RightKey = "F07.Submit", IsActive = true },      // listed, inactive row: reactivated (no duplicate)
            new { RightKey = "Settings.Admin", IsActive = true },  // listed, absent: granted
        });
        Context.BudgetRoleQueryFields.AsNoTracking().Count(x => x.RoleId == role.Id && x.IsActive).Should().Be(1);
        _scope.ActionCodes.Should().Equal(ConfigureRoleHandler.ActionCode);
        (await SnapshotAsync()).RoleOfficeLevels.Should().ContainKey("SE");
    }

    [Theory]
    [InlineData(999, "Budget.View", "category", BudgetMessages.ConfigInvalid)]         // unknown level
    [InlineData(9, "Budget.View", "category", BudgetMessages.ConfigLevelNotBudget)]    // not a Budget office level
    [InlineData(1, "No.Such.Right", "category", BudgetMessages.ConfigInvalid)]         // unknown right
    [InlineData(1, "Budget.View", "noSuchField", BudgetMessages.ConfigInvalid)]        // unknown field
    public async Task Configure_rejects_an_unknown_level_right_or_field_and_changes_nothing(
        int levelId, string right, string field, string messageId)
    {
        var role = AddRole("SE", levelId: null);

        var r = await ConfigureAsync(role.Id, levelId, [right], [field]);

        r.MessageId.Should().Be(messageId);
        ConfigEndpoints.ConfigResults(r).Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.BadRequest<object>>();
        Context.BudgetRoles.AsNoTracking().Single().OfficeLevelId.Should().BeNull();
        Context.BudgetRoleRights.Should().BeEmpty();
        _scope.ActionCodes.Should().BeEmpty();
    }

    [Fact]
    public async Task Configure_refuses_an_SSO_inactive_or_inactive_role()
    {
        var ssoOff = AddRole("SSO_OFF", null, ssoActive: false);
        var off = AddRole("OFF", null, active: false);

        (await ConfigureAsync(ssoOff.Id, DivisionLevel, ["Budget.View"], [])).MessageId.Should().Be(BudgetMessages.RoleInactive);
        (await ConfigureAsync(off.Id, DivisionLevel, ["Budget.View"], [])).MessageId.Should().Be(BudgetMessages.RoleInactive);
    }

    [Fact]
    public async Task Configure_refuses_to_remove_the_callers_own_administration_right()
    {
        var itCell = AddRole("IT_CELL", DivisionLevel, rights: "Settings.Admin");

        (await ConfigureAsync(itCell.Id, DivisionLevel, ["Budget.View"], [], actingRole: "IT_CELL"))
            .MessageId.Should().Be(BudgetMessages.ConfigSelfLockout);
        (await ConfigureAsync(itCell.Id, DivisionLevel, ["Budget.View", "Settings.Admin"], [], actingRole: "IT_CELL"))
            .IsSucced.Should().BeTrue();
    }

    // ---------------- levels.status ignored, legacy office_level kept in sync (D2, D3) ----------------

    [Fact]
    public async Task Live_like_levels_all_with_status_false_configure_every_role()
    {
        // The live master: CE, CR, Div plus the three rows the migration inserts, every one status = false.
        Context.Levels.AddRange(
            new Levels { Id = 21, Name = "CE", Status = false },
            new Levels { Id = 22, Name = "CR", Status = false },
            new Levels { Id = 23, Name = "Div", Status = false },
            new Levels { Id = 24, Name = "FINANCE", Status = false },
            new Levels { Id = 25, Name = "SECRETARIAT", Status = false },
            new Levels { Id = 26, Name = "IT_CELL", Status = false });
        await Context.SaveChangesAsync();
        AddRole("CE", 21, rights: "Budget.View");
        AddRole("SE", 22, rights: "Budget.View");
        AddRole("DDO", 23, rights: "Budget.View");
        AddRole("ProjectAdmin", 24, rights: "Settings.Admin");
        AddRole("SECRETARY", 25, rights: "Budget.View");
        AddRole("IT_CELL", 26, rights: "Settings.Admin");

        var s = await SnapshotAsync();

        s.RoleOfficeLevels.Should().BeEquivalentTo(new Dictionary<string, string>
        {
            ["CE"] = OfficeLevel.Ce, ["SE"] = OfficeLevel.Circle, ["DDO"] = OfficeLevel.Division,
            ["ProjectAdmin"] = OfficeLevel.Finance, ["SECRETARY"] = OfficeLevel.Secretariat, ["IT_CELL"] = OfficeLevel.ItCell,
        });
        s.Rights["Settings.Admin"].Should().BeEquivalentTo("ProjectAdmin", "IT_CELL");
    }

#pragma warning disable CS0618 // asserting the legacy dual-write column
    [Fact]
    public async Task Configure_accepts_a_status_false_level_and_writes_the_mapped_legacy_office_level()
    {
        var role = AddRole("SE", levelId: null);

        var r = await ConfigureAsync(role.Id, RetiredLevel, ["Budget.View"], []);

        r.IsSucced.Should().BeTrue(r.Message);
        r.Data!.IsConfigured.Should().BeTrue();
        var saved = Context.BudgetRoles.AsNoTracking().Single();
        saved.OfficeLevelId.Should().Be(RetiredLevel);
        saved.LegacyOfficeLevel.Should().Be(OfficeLevel.Circle);

        (await ConfigureAsync(role.Id, DivisionLevel, ["Budget.View"], [])).IsSucced.Should().BeTrue();
        Context.BudgetRoles.AsNoTracking().Single().LegacyOfficeLevel.Should().Be(OfficeLevel.Division,
            "the level name 'Division' is written as the code the previous release reads");
    }

    [Fact]
    public async Task A_refused_configure_leaves_the_legacy_office_level_unchanged()
    {
        var role = AddRole("SE", DivisionLevel, rights: "Budget.View");
        role.LegacyOfficeLevel = OfficeLevel.Division;
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        (await ConfigureAsync(role.Id, SubDivisionLevel, ["Budget.View"], [])).MessageId.Should().Be(BudgetMessages.ConfigLevelNotBudget);

        var saved = Context.BudgetRoles.AsNoTracking().Single();
        saved.OfficeLevelId.Should().Be(DivisionLevel);
        saved.LegacyOfficeLevel.Should().Be(OfficeLevel.Division);
    }

    [Fact]
    public async Task Legacy_upsert_by_code_maps_to_a_status_false_level_and_dual_writes()
    {
        var handler = new UpsertRoleHandler(Context, Actor("IT_CELL"), _scope, new FixedConfig(BudgetConfigSnapshot.Empty));

        var r = await handler.Handle(new UpsertRoleCommand(new UpsertRoleDto("NEW_SE", OfficeLevel.Circle, false, true)), default);

        r.IsSucced.Should().BeTrue(r.Message);
        var saved = Context.BudgetRoles.AsNoTracking().Single(x => x.RoleName == "NEW_SE");
        saved.OfficeLevelId.Should().Be(RetiredLevel);
        saved.LegacyOfficeLevel.Should().Be(OfficeLevel.Circle);
    }
#pragma warning restore CS0618

    [Fact]
    public async Task Levels_list_offers_every_Budget_level_whatever_its_status_and_hides_other_levels()
    {
        var r = await new ListLevelsHandler(Context, Actor("IT_CELL")).Handle(new ListLevelsQuery(), default);

        r.Data!.Select(l => l.LevelId).Should().BeEquivalentTo(new[] { DivisionLevel, FinanceLevel, RetiredLevel },
            "'Sub Division' is not a Budget office level; the status = false CIRCLE row is offered");
    }

    [Fact]
    public async Task Roles_list_reports_configured_state_with_the_same_rule_as_the_snapshot()
    {
        AddRole("DDO", DivisionLevel, rights: "Budget.View");
        AddRole("NEW_SYNCED", levelId: null);

        var rows = await RoleConfigReader.ReadAsync(Context, null, default);

        rows.Single(x => x.RoleName == "DDO").IsConfigured.Should().BeTrue();
        rows.Single(x => x.RoleName == "NEW_SYNCED").IsConfigured.Should().BeFalse();
        rows.Single(x => x.RoleName == "NEW_SYNCED").RightKeys.Should().BeEmpty();
    }
}
