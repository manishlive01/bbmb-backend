using BBMB.BudgetService.Authorization.RoleSource;
using BBMB.BudgetService.Features.AdminConfig;
using BBMB.BudgetService.Features.BudgetProcess.AuthzConfig;
using BBMB.BudgetService.Features.BudgetProcess.AuthzConfig.Entities;
using BBMB.BudgetService.Features.HierarchyManagement.LevelManagement.Entites;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Tests.Common;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
namespace BBMB.BudgetService.Tests.Authorization;
/// <summary>
/// The on-demand Auth SSO role sync (D3) through <see cref="SyncRolesHandler"/>, in memory. Only the SSO client
/// (a fake returning a fixed role list) and the action scope (a recorder; the real one issues PostgreSQL SQL) are
/// substituted; the merge itself is the production <see cref="BudgetRoleSync.Plan"/>.
/// </summary>
public sealed class BudgetRoleSyncTests : InMemoryTestBase
{
    private static readonly Guid ProjectId = Guid.Parse("fb9e91a1-3a86-4498-afb1-3549cba24b93");
    private static readonly Guid DdoId = Guid.NewGuid();
    private static readonly Guid SeId = Guid.NewGuid();
    private const string Token = "test-token";

    private sealed class FakeSsoRoleClient : ISsoRoleClient
    {
        public SsoRoleFetchResult Result { get; set; } = SsoRoleFetchResult.Ok([]);
        public Guid? SeenProject { get; private set; }
        public string? SeenToken { get; private set; }
        public Task<SsoRoleFetchResult> GetProjectRolesAsync(Guid projectId, string bearerToken, CancellationToken ct = default)
        {
            SeenProject = projectId;
            SeenToken = bearerToken;
            return Task.FromResult(Result);
        }
    }

    private sealed class CountingConfig : IBudgetConfigProvider
    {
        public int Invalidations { get; private set; }
        public BudgetConfigSnapshot Snapshot { get; set; } = BudgetConfigSnapshot.Empty;
        public Task<BudgetConfigSnapshot> GetAsync(CancellationToken ct = default) => Task.FromResult(Snapshot);
        public void Invalidate() => Invalidations++;
    }

    private readonly FakeSsoRoleClient _sso = new();
    private readonly FakeBudgetActionScope _scope = new();
    private readonly CountingConfig _config = new();
    private static readonly IOptions<BudgetRoleSyncOptions> Enabled = Options.Create(new BudgetRoleSyncOptions { Enabled = true });

    public BudgetRoleSyncTests()
    {
        Context.BudgetProjectSettings.Add(new BudgetProjectSettings
        {
            SsoProjectId = ProjectId, SsoProjectName = "Budget", EmployeeChargeProjectCode = "1", EditLeaseMinutes = 20,
        });
        Context.SaveChanges();
    }

    private Task<BBMB.BudgetService.Shared.ResponseDTOs.Response<RoleSyncResultDto>> SyncAsync(params SsoRole[] roles)
    {
        _sso.Result = SsoRoleFetchResult.Ok(roles);
        var http = new DefaultHttpContext();
        http.Request.Headers.Authorization = "Bearer " + Token;
        var actor = new FakeBudgetActorSource(new BudgetActor(Guid.NewGuid(), Guid.NewGuid(), 1, "IT_CELL", "IT_CELL",
            "Admin", null, FakeBudgetActorSource.TestIp, Guid.NewGuid()));
        var handler = new SyncRolesHandler(Context, actor, _scope, _config, _sso,
            new HttpContextAccessor { HttpContext = http }, Enabled, NullLogger<SyncRolesHandler>.Instance);
        return handler.Handle(new SyncRolesCommand(), default);
    }

    private BudgetRole Role(string name) => Context.BudgetRoles.AsNoTracking().Single(r => r.RoleName == name);

    [Fact]
    public async Task First_sync_creates_unconfigured_roles_through_the_audited_scope()
    {
        var r = await SyncAsync(new SsoRole(DdoId, "DDO", true), new SsoRole(SeId, "SE", true));

        r.IsSucced.Should().BeTrue(r.Message);
        r.Data.Should().Be(new RoleSyncResultDto(Created: 2, Updated: 0, Deactivated: 0, Unchanged: 0));
        var ddo = Role("DDO");
        ddo.SsoRoleId.Should().Be(DdoId);
        ddo.OfficeLevelId.Should().BeNull("a synchronised role stays unconfigured until an administrator assigns a level");
        ddo.IsActive.Should().BeTrue();
        ddo.IsAdmin.Should().BeFalse();
        ddo.LastSyncedAt.Should().NotBeNull();
        Context.BudgetRoleRights.Should().BeEmpty();
        _sso.SeenProject.Should().Be(ProjectId);
        _sso.SeenToken.Should().Be(Token, "the calling administrator's bearer token is forwarded");
        _scope.ActionCodes.Should().Equal(SyncRolesHandler.ActionCode);
        _config.Invalidations.Should().Be(1);
    }

    [Fact]
    public async Task Repeated_sync_creates_no_duplicates_and_writes_nothing()
    {
        await SyncAsync(new SsoRole(DdoId, "DDO", true));
        var stamp = Role("DDO").LastSyncedAt;

        var again = await SyncAsync(new SsoRole(DdoId, "DDO", true));

        again.Data.Should().Be(new RoleSyncResultDto(0, 0, 0, Unchanged: 1));
        Context.BudgetRoles.Count().Should().Be(1);
        Role("DDO").LastSyncedAt.Should().Be(stamp, "an unchanged row is not touched");
        _config.Invalidations.Should().Be(1, "no change, no cache invalidation");
    }

    [Fact]
    public async Task A_name_or_active_change_in_SSO_updates_the_role()
    {
        await SyncAsync(new SsoRole(DdoId, "DDO", true));

        var r = await SyncAsync(new SsoRole(DdoId, "DDO_DIVISION", false));

        r.Data.Should().Be(new RoleSyncResultDto(0, Updated: 1, 0, 0));
        var row = Context.BudgetRoles.AsNoTracking().Single();
        row.RoleName.Should().Be("DDO_DIVISION");
        row.SsoIsActive.Should().BeFalse();
    }

    [Fact]
    public async Task A_role_missing_from_SSO_is_deactivated_and_reactivated_when_it_returns()
    {
        await SyncAsync(new SsoRole(DdoId, "DDO", true), new SsoRole(SeId, "SE", true));

        (await SyncAsync(new SsoRole(DdoId, "DDO", true))).Data.Should().Be(new RoleSyncResultDto(0, 0, Deactivated: 1, Unchanged: 1));
        Role("SE").SsoIsActive.Should().BeFalse();

        (await SyncAsync(new SsoRole(DdoId, "DDO", true), new SsoRole(SeId, "SE", true))).Data!.Updated.Should().Be(1);
        Role("SE").SsoIsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Sync_never_alters_budget_owned_settings()
    {
        // A configured role: level, admin flag, a right, a query field and an admin assignment, all Budget-owned.
        var level = new Levels { Id = 5, Name = "FINANCE", Status = true };
        Context.Levels.Add(level);
        var role = new BudgetRole { RoleName = "AFA", SsoRoleId = DdoId, OfficeLevelId = 5, IsAdmin = true, IsActive = true };
        var right = new BudgetRight { RightKey = "Budget.View" };
        var field = new BudgetQueryField { FieldName = "category" };
        Context.AddRange(role, right, field);
        await Context.SaveChangesAsync();
        Context.BudgetRoleRights.Add(new BudgetRoleRight { RoleId = role.Id, RightId = right.Id });
        Context.BudgetRoleQueryFields.Add(new BudgetRoleQueryField { RoleId = role.Id, QueryFieldId = field.Id });
        Context.BudgetAdminRoleAssignments.Add(new BudgetAdminRoleAssignment { RoleId = role.Id, Status = AdminAssignmentStatus.Pending });
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // SSO renames it and marks it inactive.
        var r = await SyncAsync(new SsoRole(DdoId, "AFA_BUDGET", false));

        r.Data!.Updated.Should().Be(1);
        var after = Context.BudgetRoles.AsNoTracking().Single();
        after.RoleName.Should().Be("AFA_BUDGET");
        after.SsoIsActive.Should().BeFalse();
        after.OfficeLevelId.Should().Be(5);
        after.IsAdmin.Should().BeTrue();
        after.IsActive.Should().BeTrue("is_active is Budget-owned; SSO status lives in sso_is_active");
        Context.BudgetRoleRights.AsNoTracking().Single().IsActive.Should().BeTrue();
        Context.BudgetRoleQueryFields.AsNoTracking().Single().IsActive.Should().BeTrue();
        Context.BudgetAdminRoleAssignments.AsNoTracking().Single().Status.Should().Be(AdminAssignmentStatus.Pending);
    }

    [Fact]
    public async Task A_legacy_seeded_role_is_adopted_by_name_without_a_duplicate()
    {
        Context.BudgetRoles.Add(new BudgetRole { RoleName = "DDO", SsoRoleId = null, OfficeLevelId = null, IsActive = true });
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var r = await SyncAsync(new SsoRole(DdoId, "ddo", true));

        r.Data.Should().Be(new RoleSyncResultDto(0, Updated: 1, 0, 0));
        var row = Context.BudgetRoles.AsNoTracking().Single();
        row.SsoRoleId.Should().Be(DdoId);
        row.RoleName.Should().Be("ddo", "the SSO name is authoritative once adopted");
    }

    // Review R2-1: the previous release cannot read a synced row (office_level NULL), so the sync is off by default.
    [Fact]
    public async Task Sync_is_refused_while_the_runtime_switch_is_off_and_touches_neither_SSO_nor_the_database()
    {
        var http = new DefaultHttpContext();
        http.Request.Headers.Authorization = "Bearer " + Token;
        _sso.Result = SsoRoleFetchResult.Ok([new SsoRole(DdoId, "DDO", true)]);
        var handler = new SyncRolesHandler(Context, new FakeBudgetActorSource(new BudgetActor(Guid.NewGuid(), Guid.NewGuid(), 1,
                "IT_CELL", "IT_CELL", "Admin", null, FakeBudgetActorSource.TestIp, Guid.NewGuid())),
            _scope, _config, _sso, new HttpContextAccessor { HttpContext = http },
            Options.Create(new BudgetRoleSyncOptions()), NullLogger<SyncRolesHandler>.Instance);
        var r = await handler.Handle(new SyncRolesCommand(), default);
        r.IsSucced.Should().BeFalse();
        r.MessageId.Should().Be(BudgetMessages.RoleSyncDisabled);
        _sso.SeenProject.Should().BeNull("Auth SSO is not called while the switch is off");
        _scope.ActionCodes.Should().BeEmpty();
        Context.BudgetRoles.Should().BeEmpty();
        _config.Invalidations.Should().Be(0);
    }
    [Fact]
    public async Task An_SSO_failure_or_an_empty_role_list_changes_nothing()
    {
        await SyncAsync(new SsoRole(DdoId, "DDO", true));

        _sso.Result = SsoRoleFetchResult.Fail("HTTP_500");
        var http = new DefaultHttpContext();
        http.Request.Headers.Authorization = "Bearer " + Token;
        var handler = new SyncRolesHandler(Context, new FakeBudgetActorSource(new BudgetActor(Guid.NewGuid(), Guid.NewGuid(), 1,
                "IT_CELL", "IT_CELL", "Admin", null, FakeBudgetActorSource.TestIp, Guid.NewGuid())),
            _scope, _config, _sso, new HttpContextAccessor { HttpContext = http }, Enabled, NullLogger<SyncRolesHandler>.Instance);
        (await handler.Handle(new SyncRolesCommand(), default)).MessageId.Should().Be(BudgetMessages.SsoSyncFailed);

        (await SyncAsync()).MessageId.Should().Be(BudgetMessages.SsoSyncFailed,
            "an empty list would deactivate every role, the administrator's own included");
        Role("DDO").SsoIsActive.Should().BeTrue();
    }

    // Review finding 3: the empty-list guard does not cover a response that merely leaves out the administrator's
    // own role. The caller below acts as IT_CELL.
    [Fact]
    public async Task Sync_refuses_to_deactivate_the_callers_own_acting_role()
    {
        var itCell = Guid.NewGuid();
        await SyncAsync(new SsoRole(itCell, "IT_CELL", true), new SsoRole(DdoId, "DDO", true));
        var invalidations = _config.Invalidations;

        var r = await SyncAsync(new SsoRole(DdoId, "DDO", true));

        r.IsSucced.Should().BeFalse();
        r.MessageId.Should().Be(BudgetMessages.RoleSyncSelfLockout);
        Role("IT_CELL").SsoIsActive.Should().BeTrue("a refused sync saves nothing");
        _scope.Commits.Should().Be(1, "only the first sync committed");
        _config.Invalidations.Should().Be(invalidations);
    }

    [Fact]
    public async Task Sync_refuses_to_deactivate_the_last_administrator_role()
    {
        // The caller's own role (IT_CELL) is not a Budget row here; ProjectAdmin is the only Settings.Admin holder.
        var admin = Guid.NewGuid();
        await SyncAsync(new SsoRole(admin, "ProjectAdmin", true), new SsoRole(DdoId, "DDO", true));
        _config.Snapshot = BudgetConfigSnapshot.Empty with
        {
            Rights = new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                [ConfigureRoleHandler.AdminRightKey] = ["ProjectAdmin"],
            },
        };

        var r = await SyncAsync(new SsoRole(DdoId, "DDO", true));

        r.MessageId.Should().Be(BudgetMessages.RoleSyncSelfLockout);
        Role("ProjectAdmin").SsoIsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Sync_may_deactivate_one_administrator_role_while_another_stays_active()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        await SyncAsync(new SsoRole(a, "ProjectAdmin", true), new SsoRole(b, "FIN_ADMIN", true));
        _config.Snapshot = BudgetConfigSnapshot.Empty with { AdminRoles = ["ProjectAdmin", "FIN_ADMIN"] };

        var r = await SyncAsync(new SsoRole(b, "FIN_ADMIN", true));

        r.IsSucced.Should().BeTrue(r.Message);
        r.Data!.Deactivated.Should().Be(1);
        Role("ProjectAdmin").SsoIsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Sync_refuses_when_the_SSO_project_id_is_not_set()
    {
        var settings = Context.BudgetProjectSettings.Single();
        settings.SsoProjectId = null;
        await Context.SaveChangesAsync();

        (await SyncAsync(new SsoRole(DdoId, "DDO", true))).MessageId.Should().Be(BudgetMessages.SsoProjectUnset);
        Context.BudgetRoles.Should().BeEmpty();
    }
}
