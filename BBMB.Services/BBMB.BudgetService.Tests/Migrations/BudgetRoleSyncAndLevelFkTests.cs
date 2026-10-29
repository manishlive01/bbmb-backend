using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Npgsql;
namespace BBMB.BudgetService.Tests.Migrations;
/// <summary>
/// Migration <c>BudgetRoleSyncAndLevelFk</c> on PostgreSQL: the levels backfill, the administrator lockout guard,
/// and Down/Up on a disposable database. Each test creates its own database in the shared container.
/// </summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class BudgetRoleSyncAndLevelFkTests(BudgetDatabaseFixture fixture)
{
    private const string Before = "20261028154620_BudgetAuthzConfigTables";
    private const string Target = "20261029123546_BudgetRoleSyncAndLevelFk";
    private const string AdminGate = "role_level_unmapped_admin";
    private static readonly string[] BudgetLevelNames = ["DIVISION", "CIRCLE", "CE", "SECRETARIAT", "FINANCE", "IT_CELL"];

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

    private static Task<string?> OfficeLevelColumnAsync(string cs) => ScalarAsync(cs,
        "SELECT count(*) FROM information_schema.columns " +
        "WHERE table_schema = 'budget' AND table_name = 'budget_role' AND column_name = 'office_level'");

    [Fact]
    public async Task Admin_roles_without_a_levels_row_abort_the_migration_when_the_gate_is_unset()
    {
        var cs = await fixture.CreateFreshDatabaseAsync("rslf_lockout", GatesWithoutAdminGate);

        var act = () => fixture.MigrateToAsync(cs, Target);

        (await act.Should().ThrowAsync<PostgresException>()).Which.MessageText
            .Should().StartWith("BudgetRoleSyncAndLevelFk: no active budget.levels row for administrator role(s)");
        (await OfficeLevelColumnAsync(cs)).Should().Be("1", "the failed migration rolled back and the string column remains");
        (await ScalarAsync(cs, "SELECT count(*) FROM information_schema.columns WHERE table_schema = 'budget' " +
            "AND table_name = 'budget_role' AND column_name = 'office_level_id'")).Should().Be("0");
    }

    [Fact]
    public async Task An_inactive_levels_row_does_not_satisfy_the_admin_guard()
    {
        var cs = await fixture.CreateFreshDatabaseAsync("rslf_inactive", GatesWithoutAdminGate);
        await fixture.MigrateToAsync(cs, Before);
        foreach (var name in BudgetLevelNames)
            await ExecAsync(cs, $"INSERT INTO budget.levels (name, status) VALUES ('{name}', {(name == "IT_CELL" ? "false" : "true")})");

        var act = () => fixture.MigrateToAsync(cs, Target);

        (await act.Should().ThrowAsync<PostgresException>()).Which.MessageText.Should().Contain("IT_CELL=IT_CELL");
    }

    [Fact]
    public async Task With_the_levels_rows_present_every_seeded_role_is_backfilled_and_Down_then_Up_round_trips()
    {
        var cs = await fixture.CreateFreshDatabaseAsync("rslf_backfill", GatesWithoutAdminGate);
        await fixture.MigrateToAsync(cs, Before);
        // Mixed case and padding: the rule is trimmed and case-insensitive.
        foreach (var name in BudgetLevelNames)
            await ExecAsync(cs, $"INSERT INTO budget.levels (name, status) VALUES (' {name.ToLowerInvariant()} ', true)");
        var seededRoles = await ScalarAsync(cs, "SELECT count(*) FROM budget.budget_role");

        await fixture.MigrateToAsync(cs, Target);

        (await OfficeLevelColumnAsync(cs)).Should().Be("0", "the string column is dropped");
        (await ScalarAsync(cs, "SELECT count(*) FROM budget.budget_role WHERE office_level_id IS NULL")).Should().Be("0");
        (await ScalarAsync(cs, "SELECT upper(btrim(l.name)) FROM budget.budget_role r JOIN budget.levels l ON l.id = r.office_level_id " +
            "WHERE r.role_name = 'IT_CELL'")).Should().Be("IT_CELL");
        (await ScalarAsync(cs, "SELECT sso_project_id::text FROM budget.budget_project_settings WHERE sso_project_name = 'Budget'"))
            .Should().Be("fb9e91a1-3a86-4498-afb1-3549cba24b93");

        // Down (development databases only) restores the string from the FK.
        await fixture.MigrateToAsync(cs, Before);
        (await OfficeLevelColumnAsync(cs)).Should().Be("1");
        (await ScalarAsync(cs, "SELECT office_level FROM budget.budget_role WHERE role_name = 'IT_CELL'")).Should().Be("IT_CELL");
        (await ScalarAsync(cs, "SELECT count(*) FROM budget.budget_role WHERE office_level IS NULL")).Should().Be("0");

        // Up again.
        await fixture.MigrateToAsync(cs, Target);
        (await ScalarAsync(cs, "SELECT count(*) FROM budget.budget_role WHERE office_level_id IS NOT NULL")).Should().Be(seededRoles);
    }

    [Fact]
    public async Task With_the_gate_set_unmapped_admin_roles_are_left_unconfigured()
    {
        var cs = await fixture.CreateFreshDatabaseAsync("rslf_gated");

        await fixture.MigrateToAsync(cs, Target);

        (await ScalarAsync(cs, "SELECT count(*) FROM budget.budget_role WHERE office_level_id IS NOT NULL")).Should().Be("0",
            "budget.levels is empty in a fresh database");
    }
}
