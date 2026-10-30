using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Npgsql;
namespace BBMB.BudgetService.Tests.Migrations;
/// <summary>
/// Migration <c>BudgetRoleSyncAndLevelFk</c> on PostgreSQL: the insert-if-missing of the Budget-only levels, the
/// alias backfill, the administrator lockout guard, the kept legacy column, and Down/Up on a disposable database.
/// Each test creates its own database in the shared container. The admin gate is NOT set, so the guard is live.
/// </summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class BudgetRoleSyncAndLevelFkTests(BudgetDatabaseFixture fixture)
{
    private const string Before = "20261028154620_BudgetAuthzConfigTables";
    private const string Target = "20261030040709_BudgetRoleSyncAndLevelFk";
    private const string AdminGate = "role_level_unmapped_admin";
    private static IEnumerable<string> GatesWithoutAdminGate => BudgetDatabaseFixture.AllGates.Where(g => g != AdminGate);
    private static async Task<string?> ScalarAsync(string cs, string sql)
    {
        await using var conn = new NpgsqlConnection(cs);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        var v = await cmd.ExecuteScalarAsync();
        return v is null or DBNull ? null : v.ToString();
    }
    private static async Task ExecAsync(string cs, string sql)
    {
        await using var conn = new NpgsqlConnection(cs);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync();
    }
    /// <summary>An UPDATE of budget_role under a test actor (the row triggers require one).</summary>
    private async Task ExecAsActorAsync(string cs, string sql)
    {
        var officeId = await DbActor.ItCellOfficeIdAsync(cs);
        await using var conn = new NpgsqlConnection(cs);
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();
        await DbActor.SetAsync(conn, tx, fixture.MigrationEmployeeId, fixture.MigrationChargeId, officeId);
        await using (var cmd = new NpgsqlCommand(sql, conn, tx))
            await cmd.ExecuteNonQueryAsync();
        await tx.CommitAsync();
    }
    private static Task<string?> OfficeLevelNullableAsync(string cs) => ScalarAsync(cs,
        "SELECT is_nullable FROM information_schema.columns " +
        "WHERE table_schema = 'budget' AND table_name = 'budget_role' AND column_name = 'office_level'");

    /// <summary>
    /// The live NIC NET shape (read-only check, 29 Oct 2026): CE, CR, Div with ids 1..3, every row status = false,
    /// loaded with explicit ids so the identity sequence still stands at 1, not called.
    /// </summary>
    private static Task SeedLiveLikeLevelsAsync(string cs) => ExecAsync(cs,
        "INSERT INTO budget.levels (id, name, status) VALUES (1, 'CE', false), (2, 'CR', false), (3, 'Div', false)");

    [Fact]
    public async Task Live_like_levels_pass_the_guard_map_every_role_and_insert_only_the_missing_rows()
    {
        var cs = await fixture.CreateFreshDatabaseAsync("rslf_live", GatesWithoutAdminGate);
        await fixture.MigrateToAsync(cs, Before);
        await SeedLiveLikeLevelsAsync(cs);
        var seededRoles = await ScalarAsync(cs, "SELECT count(*) FROM budget.budget_role");
        var legacyBefore = await ScalarAsync(cs,
            "SELECT string_agg(role_name || '=' || office_level, ',' ORDER BY id) FROM budget.budget_role");

        await fixture.MigrateToAsync(cs, Target);

        (await ScalarAsync(cs, "SELECT count(*) FROM budget.budget_role WHERE office_level_id IS NOT NULL")).Should().Be(seededRoles,
            "with CE, CR, Div plus the three inserted rows every seeded role maps, although every level has status = false");
        (await ScalarAsync(cs, "SELECT string_agg(r.role_name || '=' || l.name, ',' ORDER BY r.id) " +
            "FROM budget.budget_role r JOIN budget.levels l ON l.id = r.office_level_id"))
            .Should().Be("DDO=Div,SE=CR,CE=CE,SECRETARY=SECRETARIAT,AFA_BUDGET=FINANCE,FIN_VIEW=FINANCE,IT_CELL=IT_CELL,ProjectAdmin=FINANCE");
        (await ScalarAsync(cs, "SELECT string_agg(id || ':' || name || ':' || status, ',' ORDER BY id) FROM budget.levels"))
            .Should().Be("1:CE:false,2:CR:false,3:Div:false,4:FINANCE:false,5:SECRETARIAT:false,6:IT_CELL:false",
                "existing rows are untouched and the new rows take ids past the stale identity sequence");
        (await ScalarAsync(cs, "SELECT string_agg(role_name || '=' || office_level, ',' ORDER BY id) FROM budget.budget_role"))
            .Should().Be(legacyBefore, "the legacy office_level string is kept for the previous release");
        (await OfficeLevelNullableAsync(cs)).Should().Be("YES");
        (await ScalarAsync(cs, "SELECT count(*) FROM pg_constraint WHERE conname = 'ck_budget_role_office_level'")).Should().Be("1");
    }

    [Fact]
    public async Task Existing_Budget_level_rows_are_not_inserted_twice_whatever_their_case()
    {
        var cs = await fixture.CreateFreshDatabaseAsync("rslf_exists", GatesWithoutAdminGate);
        await fixture.MigrateToAsync(cs, Before);
        await ExecAsync(cs, "INSERT INTO budget.levels (name, status) VALUES (' finance ', true), ('It_Cell', false)");

        await fixture.MigrateToAsync(cs, Target);

        (await ScalarAsync(cs, "SELECT string_agg(name || ':' || status, ',' ORDER BY id) FROM budget.levels"))
            .Should().Be(" finance :true,It_Cell:false,SECRETARIAT:false", "only the missing SECRETARIAT row is inserted; no row is updated");
        (await ScalarAsync(cs, "SELECT l.name FROM budget.budget_role r JOIN budget.levels l ON l.id = r.office_level_id " +
            "WHERE r.role_name = 'ProjectAdmin'")).Should().Be(" finance ");
    }

    [Fact]
    public async Task An_admin_role_whose_level_has_no_levels_row_aborts_the_migration()
    {
        var cs = await fixture.CreateFreshDatabaseAsync("rslf_lockout", GatesWithoutAdminGate);
        await fixture.MigrateToAsync(cs, Before);
        // budget.levels is empty and the migration inserts only FINANCE, SECRETARIAT and IT_CELL: an admin at CE
        // finds no row.
        await ExecAsActorAsync(cs, "UPDATE budget.budget_role SET office_level = 'CE' WHERE role_name = 'ProjectAdmin'");

        var act = () => fixture.MigrateToAsync(cs, Target);

        (await act.Should().ThrowAsync<PostgresException>()).Which.MessageText
            .Should().StartWith("BudgetRoleSyncAndLevelFk: no budget.levels row for administrator role(s) ProjectAdmin=CE");
        (await ScalarAsync(cs, "SELECT count(*) FROM information_schema.columns WHERE table_schema = 'budget' " +
            "AND table_name = 'budget_role' AND column_name = 'office_level_id'")).Should().Be("0", "the failed migration rolled back");
        (await ScalarAsync(cs, "SELECT count(*) FROM budget.levels")).Should().Be("0", "the level inserts rolled back too");
    }

    [Fact]
    public async Task Down_then_Up_round_trips_without_duplicating_levels()
    {
        var cs = await fixture.CreateFreshDatabaseAsync("rslf_roundtrip", GatesWithoutAdminGate);
        await fixture.MigrateToAsync(cs, Before);
        await SeedLiveLikeLevelsAsync(cs);
        await fixture.MigrateToAsync(cs, Target);
        (await ScalarAsync(cs, "SELECT sso_project_id::text FROM budget.budget_project_settings WHERE sso_project_name = 'Budget'"))
            .Should().Be("fb9e91a1-3a86-4498-afb1-3549cba24b93");

        // Down (development databases only): office_level back to NOT NULL, inserted levels kept.
        await fixture.MigrateToAsync(cs, Before);
        (await OfficeLevelNullableAsync(cs)).Should().Be("NO");
        (await ScalarAsync(cs, "SELECT office_level FROM budget.budget_role WHERE role_name = 'IT_CELL'")).Should().Be("IT_CELL");
        (await ScalarAsync(cs, "SELECT count(*) FROM budget.levels")).Should().Be("6");

        await fixture.MigrateToAsync(cs, Target);
        (await ScalarAsync(cs, "SELECT count(*) FROM budget.levels")).Should().Be("6", "the inserts are insert-if-missing");
        (await ScalarAsync(cs, "SELECT count(*) FROM budget.budget_role WHERE office_level_id IS NULL")).Should().Be("0");
    }

    [Fact]
    public async Task On_a_fresh_database_only_roles_on_the_inserted_levels_are_mapped()
    {
        var cs = await fixture.CreateFreshDatabaseAsync("rslf_fresh", GatesWithoutAdminGate);
        await fixture.MigrateToAsync(cs, Target);
        (await ScalarAsync(cs, "SELECT string_agg(role_name, ',' ORDER BY id) FROM budget.budget_role WHERE office_level_id IS NULL"))
            .Should().Be("DDO,SE,CE", "no hierarchy row exists for DIVISION, CIRCLE or CE; those roles stay unconfigured");
    }
}
