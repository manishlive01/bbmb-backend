using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Npgsql;

namespace BBMB.BudgetService.Tests.Migrations;

/// <summary>
/// Database-layer negative tests for the actor/audit objects of BudgetP1ReferenceStateAndAudit
/// (DDD DB-13, DB-27, DB-28, DB-29, DV-05, DV-06; design §4.3, §11.2). They pass only when the database refuses.
/// The test connection is the table owner (postgres), so refusals are by trigger, not by privilege.
/// </summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class AuditLayerNegativeTests(BudgetDatabaseFixture fixture)
{
    private const string InsertFinance =
        "INSERT INTO budget.office (office_level, office_name) VALUES ('FINANCE', 'Finance (test)')";

    private async Task<NpgsqlConnection> OpenAsync()
    {
        var conn = new NpgsqlConnection(fixture.PlainConnectionString);
        await conn.OpenAsync();
        return conn;
    }

    private static async Task ExecAsync(NpgsqlConnection conn, string sql, NpgsqlTransaction? tx = null)
    {
        await using var cmd = new NpgsqlCommand(sql, conn, tx);
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task<long> CountAsync(NpgsqlConnection conn, string sql, NpgsqlTransaction? tx = null)
    {
        await using var cmd = new NpgsqlCommand(sql, conn, tx);
        return Convert.ToInt64(await cmd.ExecuteScalarAsync());
    }

    private static async Task<PostgresException> RefusedAsync(Func<Task> act)
    {
        var ex = (await act.Should().ThrowAsync<PostgresException>()).Which;
        ex.SqlState.Should().Be(PostgresErrorCodes.RaiseException);
        return ex;
    }

    [Fact]
    public async Task DV05_TruncateAuditLogRefusedAsOwner()
    {
        await using var conn = await OpenAsync();
        var before = await CountAsync(conn, "SELECT count(*) FROM budget.audit_log");
        var ex = await RefusedAsync(() => ExecAsync(conn, "TRUNCATE budget.audit_log"));
        ex.MessageText.Should().Be("FORBIDDEN");
        (await CountAsync(conn, "SELECT count(*) FROM budget.audit_log")).Should().Be(before).And.BeGreaterThan(0);
    }

    [Fact]
    public async Task DB27_UpdateAuditLogRefusedAsOwner()
    {
        await using var conn = await OpenAsync();
        (await RefusedAsync(() => ExecAsync(conn, "UPDATE budget.audit_log SET actor_name = 'tampered'")))
            .MessageText.Should().Be("FORBIDDEN");
        (await CountAsync(conn, "SELECT count(*) FROM budget.audit_log WHERE actor_name = 'tampered'")).Should().Be(0);
    }

    [Fact]
    public async Task DB27_DeleteAuditLogRefusedAsOwner()
    {
        await using var conn = await OpenAsync();
        var before = await CountAsync(conn, "SELECT count(*) FROM budget.audit_log");
        (await RefusedAsync(() => ExecAsync(conn, "DELETE FROM budget.audit_log WHERE audit_id = (SELECT min(audit_id) FROM budget.audit_log)")))
            .MessageText.Should().Be("FORBIDDEN");
        (await CountAsync(conn, "SELECT count(*) FROM budget.audit_log")).Should().Be(before);
    }

    [Fact]
    public async Task DB27_seeded_state_and_setting_rows_cannot_be_updated_deleted_or_truncated()
    {
        await using var conn = await OpenAsync();
        await RefusedAsync(() => ExecAsync(conn, "UPDATE budget.state SET figures_open = NOT figures_open WHERE state_code = 'S-02'"));
        await RefusedAsync(() => ExecAsync(conn, "DELETE FROM budget.setting"));
        await RefusedAsync(() => ExecAsync(conn, "TRUNCATE budget.state CASCADE"));
        await RefusedAsync(() => ExecAsync(conn, "TRUNCATE budget.office CASCADE"));
        (await CountAsync(conn, "SELECT count(*) FROM budget.state")).Should().Be(16);
    }

    [Fact]
    public async Task DB28_InsertWithoutActorContextRefused()
    {
        await using var conn = await OpenAsync();
        var ex = await RefusedAsync(() => ExecAsync(conn, InsertFinance));
        ex.MessageText.Should().Be("ACTOR");
        (await CountAsync(conn, "SELECT count(*) FROM budget.office WHERE office_name = 'Finance (test)'")).Should().Be(0);
    }

    [Fact]
    public async Task DB28_direct_insert_into_audit_log_without_context_refused()
    {
        await using var conn = await OpenAsync();
        var ex = await RefusedAsync(() => ExecAsync(conn,
            "INSERT INTO budget.audit_log (table_name, record_key, operation, actor_employee_id, actor_name, " +
            $"actor_charge_id, actor_office_id, actor_role_code) VALUES ('x', '1', 'I', '{fixture.MigrationEmployeeId}', " +
            $"'forged', '{fixture.MigrationChargeId}', (SELECT office_id FROM budget.office WHERE office_level = 'IT_CELL'), 'X')"));
        ex.MessageText.Should().Be("ACTOR");
    }

    [Fact]
    public async Task DB28_ExpiredByTimeChargeRefused()
    {
        // is_charge_expire is still false: only the time test catches it (D-12).
        var (emp, charge, _) = await DbActor.NewEmployeeWithChargeAsync(fixture,
            chargeExpireTime: DateTime.UtcNow.AddMinutes(-5), isChargeExpire: false);
        var itCell = await DbActor.ItCellOfficeIdAsync(fixture.PlainConnectionString);

        await using var conn = await OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();
        await DbActor.SetAsync(conn, tx, emp, charge, itCell);
        var ex = await RefusedAsync(() => ExecAsync(conn, InsertFinance, tx));
        ex.MessageText.Should().Be("ACTOR");
    }

    [Fact]
    public async Task DB28_charge_of_another_employee_refused()
    {
        var (emp, _, _) = await DbActor.NewEmployeeWithChargeAsync(fixture);
        var itCell = await DbActor.ItCellOfficeIdAsync(fixture.PlainConnectionString);

        await using var conn = await OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();
        await DbActor.SetAsync(conn, tx, emp, fixture.MigrationChargeId, itCell);
        (await RefusedAsync(() => ExecAsync(conn, InsertFinance, tx))).MessageText.Should().Be("ACTOR");
    }

    [Fact]
    public async Task DB29_insert_and_update_are_audited_and_creation_stamp_cannot_be_altered()
    {
        var (emp, charge, _) = await DbActor.NewEmployeeWithChargeAsync(fixture, name: "Officer A");
        var itCell = await DbActor.ItCellOfficeIdAsync(fixture.PlainConnectionString);

        await using var conn = await OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();
        await DbActor.SetAsync(conn, tx, emp, charge, itCell, action: "TEST_INSERT", name: "Officer A");
        await using var ins = new NpgsqlCommand(InsertFinance + " RETURNING office_id", conn, tx);
        var id = (long)(await ins.ExecuteScalarAsync())!;

        // The application never writes created_*: an attempt to rewrite them is reset to the OLD values.
        await ExecAsync(conn,
            $"UPDATE budget.office SET office_name = 'Finance (renamed)', created_by = '{fixture.MigrationEmployeeId}', " +
            $"created_at = '2000-01-01' WHERE office_id = {id}", tx);

        (await CountAsync(conn, $"SELECT count(*) FROM budget.office WHERE office_id = {id} AND created_by = '{emp}' " +
                                $"AND created_at > '2001-01-01' AND updated_by = '{emp}' AND updated_charge_id = '{charge}'", tx))
            .Should().Be(1);
        (await CountAsync(conn, $"SELECT count(*) FROM budget.audit_log WHERE table_name = 'office' AND record_key = '{id}' " +
                                $"AND operation = 'I' AND actor_name = 'Officer A' AND client_ip = '192.0.2.1'", tx))
            .Should().Be(1);
        (await CountAsync(conn, $"SELECT count(*) FROM budget.audit_log WHERE table_name = 'office' AND record_key = '{id}' " +
                                $"AND operation = 'U' AND column_name = 'office_name' " +
                                $"AND old_value = '\"Finance (test)\"' AND new_value = '\"Finance (renamed)\"'", tx))
            .Should().Be(1);
        (await CountAsync(conn, $"SELECT count(*) FROM budget.audit_log WHERE table_name = 'office' AND record_key = '{id}' " +
                                $"AND column_name IN ('created_by', 'created_at')", tx))
            .Should().Be(0, "the creation stamp did not change");

        await tx.RollbackAsync(); // leave the shared database as it was
    }

    [Fact]
    public async Task DV06_context_does_not_survive_the_transaction_on_the_same_connection()
    {
        // Own database: this test commits a row.
        var cs = await fixture.CreateFreshDatabaseAsync("dv06");
        await fixture.MigrateToAsync(cs, null);
        var itCell = await DbActor.ItCellOfficeIdAsync(cs);
        var emp = fixture.MigrationEmployeeId;
        var charge = fixture.MigrationChargeId;

        await using var conn = new NpgsqlConnection(new NpgsqlConnectionStringBuilder(cs) { Pooling = false }.ConnectionString);
        await conn.OpenAsync();
        long id;
        await using (var tx = await conn.BeginTransactionAsync())
        {
            await DbActor.SetAsync(conn, tx, emp, charge, itCell, name: "Officer A");
            await using var cmd = new NpgsqlCommand(InsertFinance + " RETURNING office_id", conn, tx);
            id = (long)(await cmd.ExecuteScalarAsync())!;
            await tx.CommitAsync();
        }

        // Same physical connection, new transaction, no context: refused, and nothing attributed to A.
        await using (var tx2 = await conn.BeginTransactionAsync())
        {
            (await RefusedAsync(() => ExecAsync(conn,
                $"UPDATE budget.office SET office_name = 'Finance (by nobody)' WHERE office_id = {id}", tx2)))
                .MessageText.Should().Be("ACTOR");
        }
        (await CountAsync(conn, $"SELECT count(*) FROM budget.audit_log WHERE table_name = 'office' " +
                                $"AND record_key = '{id}' AND operation = 'U'")).Should().Be(0);

    }
}
