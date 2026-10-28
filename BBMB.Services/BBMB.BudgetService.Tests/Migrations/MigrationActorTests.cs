using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;

namespace BBMB.BudgetService.Tests.Migrations;

/// <summary>Design §11.1 rule 8 and §14 (migration actor tests); DDD §9.</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class MigrationActorTests(BudgetDatabaseFixture fixture)
{
    private const string BeforeAudit = "20261014133303_BudgetP1RupeesAndMoneyColumns";

    /// <summary>
    /// Every migration that writes data, with the tables it writes. Extended by each phase that adds a
    /// data-shaping migration, so the one-run test below covers several migrations.
    /// </summary>
    private static readonly (string Migration, string[] Tables)[] DataShapingMigrations =
    [
        ("BudgetP1ReferenceStateAndAudit", ["office", "state", "setting"]),
        ("BudgetP1ProposalSpineMasters", ["budget_head_master", "work"]),
        ("BudgetP1StateTransitionSeed", ["state_transition"]),
        // P4-01 copies work_code -> work_code_prev and resets work_code, under its own migration actor; only
        // `work` carries the audit trigger (estimate is legacy, outside T-01..T-51, DV-19).
        ("BudgetP4WorkCodeConformance", ["work"]),
    ];

    private static async Task<string?> ScalarAsync(string cs, string sql)
    {
        await using var conn = new NpgsqlConnection(cs);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        var v = await cmd.ExecuteScalarAsync();
        return v is null or DBNull ? null : v.ToString();
    }

    [Fact]
    public async Task Rule8_run_without_Options_aborts_with_migration_actor_absent()
    {
        var cs = await fixture.CreateFreshDatabaseAsync("ma_no_options");
        var act = () => fixture.MigrateToAsync(cs, null, withMigrationActor: false);
        (await act.Should().ThrowAsync<PostgresException>()).Which.MessageText
            .Should().Be("migration actor absent or inactive");

        (await ScalarAsync(cs, "SELECT to_regclass('budget.audit_log')::text")).Should().BeNull("the migration rolled back");
        (await ScalarAsync(cs, "SELECT to_regtype('budget.rupees')::text")).Should().Be("budget.rupees",
            "the earlier migration, which writes no data, was applied");
    }

    [Fact]
    public async Task Rule8_database_level_setting_aborts_with_set_at_database_or_role_level()
    {
        var cs = await fixture.CreateFreshDatabaseAsync("ma_db_level");
        await using (var conn = new NpgsqlConnection(cs))
        {
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(
                $"ALTER DATABASE ma_db_level SET \"bbmb_migration.actor_employee_id\" = '{fixture.MigrationEmployeeId}'", conn);
            await cmd.ExecuteNonQueryAsync();
        }

        var act = () => fixture.MigrateToAsync(cs, null);
        (await act.Should().ThrowAsync<PostgresException>()).Which.MessageText
            .Should().Be("migration actor set at database or role level");
        (await ScalarAsync(cs, "SELECT to_regclass('budget.audit_log')::text")).Should().BeNull();
    }

    [Fact]
    public async Task Rule8_officer_whose_charge_expired_by_time_is_refused()
    {
        var cs = await fixture.CreateFreshDatabaseAsync("ma_expired");
        await fixture.MigrateToAsync(cs, BeforeAudit);

        // The officer and charge live in the fresh database's employee schema.
        await using var emp = BudgetDatabaseFixture.NewEmployeeContext(cs);
        var officer = await emp.Employees.AsNoTracking().FirstAsync();
        var charge = await emp.EmployeeCharges.AsNoTracking().FirstAsync(c => c.EmployeeId == officer.Id);
        await emp.Database.ExecuteSqlRawAsync(
            "UPDATE employee.employee_charge SET charge_expire_time = now() - interval '1 minute' WHERE id = {0}", charge.Id);

        await using var ctx = BudgetDatabaseFixture.NewBudgetContext(
            BudgetDatabaseFixture.MigrationConnectionString(cs, officer.Id, charge.Id));
        var act = () => ctx.GetService<IMigrator>().MigrateAsync();
        (await act.Should().ThrowAsync<PostgresException>()).Which.MessageText
            .Should().Be("migration actor absent or inactive");
    }

    [Fact]
    public async Task Rule8_each_migration_in_one_run_carries_its_own_action_and_each_step_resets_the_context()
    {
        var cs = await fixture.CreateFreshDatabaseAsync("ma_one_run");
        await fixture.MigrateToAsync(cs, BeforeAudit);
        // Legacy rows for the later data-shaping migrations to backfill.
        var h = await TestMasters.InsertDivisionAsync(cs);
        var seg = await TestMasters.InsertSegmentsAsync(cs);
        await using (var conn = new NpgsqlConnection(cs))
        {
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(
                "INSERT INTO budget.budget_head_master (major_acc_code, sub_major_acc_code, minor_acc_code, sub_minor_acc_code, " +
                "detail_acc_code, object_acc_code, head_desc, modification_date, user_code) " +
                $"VALUES ('{seg.Major}', '{seg.SubMajor}', '{seg.Minor}', '{seg.SubMinor}', '{seg.Detail}', '{seg.Object}', 'h', now(), 'L'); " +
                "INSERT INTO budget.work (work_code, division_id, major_acc_code, sub_major_acc_code, minor_acc_code, " +
                "sub_minor_acc_code, detail_acc_code, object_acc_code, original_budget, revised_budget, proposed_budget, " +
                $"remarks, work_desc, user_code) VALUES (1, {h.DivisionId}, '{seg.Major}', '{seg.SubMajor}', '{seg.Minor}', " +
                $"'{seg.SubMinor}', '{seg.Detail}', '{seg.Object}', '', '', '', '', 'w', 'L')", conn);
            await cmd.ExecuteNonQueryAsync();
        }
        // Every remaining migration in one MigrateAsync call.
        await fixture.MigrateToAsync(cs, null);

        await using var ctx = BudgetDatabaseFixture.NewBudgetContext(cs);
        var rows = await ctx.AuditLog.AsNoTracking().ToListAsync();
        rows.Should().NotBeEmpty();
        rows.Should().OnlyContain(r => r.ActionCode!.StartsWith("MIGRATION:"), "only migrations have written");
        rows.Should().OnlyContain(r => r.ActorRoleCode == "MIGRATION" && r.ActorName == BudgetDatabaseFixture.MigrationEmployeeName);

        foreach (var (migration, tables) in DataShapingMigrations)
        {
            var own = rows.Where(r => r.ActionCode == $"MIGRATION:{migration}").ToList();
            own.Select(r => r.TableName).Distinct().Should().BeEquivalentTo(tables, migration);
        }
        rows.Select(r => r.ActionCode).Distinct()
            .Should().BeEquivalentTo(DataShapingMigrations.Select(m => $"MIGRATION:{m.Migration}"));

        // Each data-shaping step re-ran MigrationActor.sql: a fresh request id per step.
        var stateReq = rows.Where(r => r.TableName == "state").Select(r => r.RequestId).Distinct().ToList();
        var settingReq = rows.Where(r => r.TableName == "setting").Select(r => r.RequestId).Distinct().ToList();
        stateReq.Should().ContainSingle();
        settingReq.Should().ContainSingle();
        stateReq[0].Should().NotBe(settingReq[0]!.Value);
    }
}
