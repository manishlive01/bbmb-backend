using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Npgsql;

namespace BBMB.BudgetService.Tests.Migrations;

/// <summary>
/// Database-layer tests for BudgetP1ProposalSpine (DDD DB-01, DB-03, DB-04, DB-10, DB-11, DB-13, DB-14,
/// DB-15, DV-05; design §5.3, §5.5 row d, §11.2). They pass only when the database refuses. The connection
/// is the table owner, so every refusal is by trigger.
/// </summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class BudgetP1ProposalSpineTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;

    public async Task InitializeAsync() => _w = await SpineWorld.CreateAsync(fixture, $"spine_{Guid.NewGuid():N}"[..22]);

    public Task DisposeAsync() => Task.CompletedTask;

    private static async Task<long> ExecScalarAsync(NpgsqlConnection c, NpgsqlTransaction tx, string sql)
    {
        await using var cmd = new NpgsqlCommand(sql, c, tx);
        var v = await cmd.ExecuteScalarAsync();
        return v is null or DBNull ? 0 : Convert.ToInt64(v);
    }

    private static async Task ExecAsync(NpgsqlConnection c, NpgsqlTransaction tx, string sql)
    {
        await using var cmd = new NpgsqlCommand(sql, c, tx);
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task<PostgresException> RefusedAsync(Func<Task> act)
    {
        var ex = (await act.Should().ThrowAsync<PostgresException>()).Which;
        return ex;
    }

    private string InsertWorkLine(int workId, int serial, long? proposalId = null, decimal rbe = 100) =>
        "INSERT INTO budget.demand_line (proposal_id, line_type, serial_no, work_id, account_code_id, rbe_amount, obe_next_amount) " +
        $"VALUES ({proposalId ?? _w.WorksProposalAId}, 'WORK', {serial}, {workId}, {_w.AccountCodeId}, {rbe}, 50) RETURNING demand_line_id";

    /// <summary>Moves the WORKS proposal of Division A from S-01 to S-02 with its movement row (DB-10 positive path).</summary>
    private async Task<long> SubmitProposalAAsync()
    {
        var (c, tx) = await _w.BeginAsync(_w.DivisionA, "SUBMIT");
        await using var _ = c;
        var movement = await ExecScalarAsync(c, tx,
            "INSERT INTO budget.proposal_movement (proposal_id, from_state, action_code, to_state, from_office_id, to_office_id, role_code, client_ip) " +
            $"VALUES ({_w.WorksProposalAId}, 'S-01', 'SUBMIT', 'S-02', {_w.DivisionAOfficeId}, {_w.CircleOfficeId}, 'DDO', '192.0.2.1') RETURNING movement_id");
        await ExecAsync(c, tx,
            $"UPDATE budget.proposal SET state_code = 'S-02', holder_office_id = {_w.CircleOfficeId} WHERE proposal_id = {_w.WorksProposalAId}");
        await tx.CommitAsync();
        return movement;
    }

    [Fact]
    public async Task DV05_figure_version_and_proposal_movement_refuse_truncate_update_and_delete()
    {
        var (c, tx) = await _w.BeginAsync(_w.DivisionA);
        await using var _ = c;
        await ExecScalarAsync(c, tx, InsertWorkLine(_w.WorkAId, 1));
        await tx.CommitAsync();
        var movement = await SubmitProposalAAsync();

        foreach (var sql in new[]
                 {
                     "TRUNCATE budget.figure_version CASCADE", "TRUNCATE budget.proposal_movement CASCADE",
                     "UPDATE budget.figure_version SET value = 1", "DELETE FROM budget.figure_version",
                     $"UPDATE budget.proposal_movement SET remark = 'x' WHERE movement_id = {movement}",
                     "DELETE FROM budget.proposal_movement",
                 })
        {
            var (c2, tx2) = await _w.BeginAsync(_w.ItCell);
            await using var __ = c2;
            (await RefusedAsync(() => ExecAsync(c2, tx2, sql))).MessageText.Should().Be("FORBIDDEN", sql);
        }
    }

    [Fact]
    public async Task DB14_changing_rbe_writes_one_figure_version_with_C_columns_and_its_own_audit_row()
    {
        var (c, tx) = await _w.BeginAsync(_w.DivisionA, "DEMAND_SAVE");
        await using var _ = c;
        var line = await ExecScalarAsync(c, tx, InsertWorkLine(_w.WorkAId, 1));
        (await ExecScalarAsync(c, tx, $"SELECT count(*) FROM budget.figure_version WHERE demand_line_id = {line}"))
            .Should().Be(2, "an inserted line versions both figures");

        await ExecAsync(c, tx, $"UPDATE budget.demand_line SET rbe_amount = 250 WHERE demand_line_id = {line}");
        (await ExecScalarAsync(c, tx,
                $"SELECT count(*) FROM budget.figure_version WHERE demand_line_id = {line} AND figure = 'RBE' AND value = 250 " +
                $"AND created_by = '{_w.DivisionA.Actor!.EmployeeId}' AND created_charge_id = '{_w.DivisionA.Actor.ChargeId}' " +
                $"AND level_office_id = {_w.DivisionAOfficeId} AND state_code = 'S-01' AND change_source = 'ENTRY' AND role_code = 'DDO'"))
            .Should().Be(1);
        (await ExecScalarAsync(c, tx, $"SELECT count(*) FROM budget.figure_version WHERE demand_line_id = {line}"))
            .Should().Be(3, "only the changed figure is versioned");
        (await ExecScalarAsync(c, tx,
                "SELECT count(*) FROM budget.audit_log a JOIN budget.figure_version f ON a.record_key = f.figure_version_id::text " +
                $"WHERE a.table_name = 'figure_version' AND a.operation = 'I' AND f.demand_line_id = {line}"))
            .Should().Be(3, "every figure version has its T-51 insert row");
    }

    [Fact]
    public async Task DB10_state_change_without_a_movement_row_is_refused()
    {
        var (c, tx) = await _w.BeginAsync(_w.DivisionA);
        await using var _ = c;
        await ExecAsync(c, tx, $"UPDATE budget.proposal SET state_code = 'S-02' WHERE proposal_id = {_w.WorksProposalAId}");
        (await RefusedAsync(() => tx.CommitAsync())).MessageText.Should().Be("DB-10", "the check is deferred to commit");
    }

    [Fact]
    public async Task DB10_state_change_with_its_movement_row_commits()
    {
        await SubmitProposalAAsync();
        var (c, tx) = await _w.BeginAsync(_w.ItCell);
        await using var _ = c;
        (await ExecScalarAsync(c, tx, $"SELECT count(*) FROM budget.proposal WHERE proposal_id = {_w.WorksProposalAId} " +
                                      $"AND state_code = 'S-02' AND holder_office_id = {_w.CircleOfficeId}")).Should().Be(1);
    }

    [Fact]
    public async Task DB11_figure_change_refused_when_state_is_closed_for_figures()
    {
        var (c, tx) = await _w.BeginAsync(_w.DivisionA);
        long line;
        await using (c)
        {
            line = await ExecScalarAsync(c, tx, InsertWorkLine(_w.WorkAId, 1));
            await tx.CommitAsync();
        }
        await SubmitProposalAAsync(); // S-02: figures_open = false; holder the Circle

        var (c2, tx2) = await _w.BeginAsync(_w.Circle);
        await using var _ = c2;
        (await RefusedAsync(() => ExecAsync(c2, tx2, $"UPDATE budget.demand_line SET rbe_amount = 1 WHERE demand_line_id = {line}")))
            .MessageText.Should().Be("M-19");
    }

    [Fact]
    public async Task DB11_figure_change_by_an_office_other_than_the_holder_is_refused()
    {
        var (c, tx) = await _w.BeginAsync(_w.DivisionB);
        await using var _ = c;
        (await RefusedAsync(() => ExecScalarAsync(c, tx, InsertWorkLine(_w.WorkAId, 1)))).MessageText.Should().Be("M-20");
    }

    [Fact]
    public async Task DB11_figure_change_refused_once_the_cycle_is_frozen()
    {
        var (c, tx) = await _w.BeginAsync(_w.ItCell);
        await using (c)
        {
            await ExecAsync(c, tx, $"UPDATE budget.budget_cycle SET status = 'FROZEN' WHERE cycle_id = {_w.CycleId}");
            await tx.CommitAsync();
        }
        var (c2, tx2) = await _w.BeginAsync(_w.DivisionA);
        await using var _ = c2;
        (await RefusedAsync(() => ExecScalarAsync(c2, tx2, InsertWorkLine(_w.WorkAId, 1)))).MessageText.Should().Be("M-17");
    }

    [Fact]
    public async Task DB13_a_deleted_line_cannot_be_restored_and_deletion_is_stamped()
    {
        var (c, tx) = await _w.BeginAsync(_w.DivisionA);
        await using var _ = c;
        var line = await ExecScalarAsync(c, tx, InsertWorkLine(_w.WorkAId, 1));
        await ExecAsync(c, tx, $"UPDATE budget.demand_line SET is_deleted = true WHERE demand_line_id = {line}");
        (await ExecScalarAsync(c, tx, $"SELECT count(*) FROM budget.demand_line WHERE demand_line_id = {line} " +
                                      $"AND deleted_by = '{_w.DivisionA.Actor!.EmployeeId}' AND deleted_at IS NOT NULL")).Should().Be(1);
        (await RefusedAsync(() => ExecAsync(c, tx, $"UPDATE budget.demand_line SET is_deleted = false WHERE demand_line_id = {line}")))
            .MessageText.Should().Be("DB-13");
    }

    [Fact]
    public async Task DB15_observation_is_fixed_reply_once_and_return_columns_once_inside_RETURN()
    {
        var movement = await SubmitProposalAAsync(); // any movement id satisfies the FK for the return column test

        var (c, tx) = await _w.BeginAsync(_w.Circle, "OBSERVE");
        await using var _ = c;
        var obs = await ExecScalarAsync(c, tx,
            "INSERT INTO budget.observation (proposal_id, level_office_id, observation_text) " +
            $"VALUES ({_w.WorksProposalAId}, {_w.CircleOfficeId}, 'Please justify') RETURNING observation_id");

        (await RefusedAsync(() => ExecAsync(c, tx, $"UPDATE budget.observation SET observation_text = 'changed' WHERE observation_id = {obs}")))
            .MessageText.Should().Be("DB-15");
        await tx.RollbackAsync();

        // Outside a RETURN the return columns are refused.
        (c, tx) = await _w.BeginAsync(_w.Circle, "OBSERVE");
        obs = await ExecScalarAsync(c, tx,
            "INSERT INTO budget.observation (proposal_id, level_office_id, observation_text) " +
            $"VALUES ({_w.WorksProposalAId}, {_w.CircleOfficeId}, 'Please justify') RETURNING observation_id");
        await tx.CommitAsync();
        await c.DisposeAsync();

        (c, tx) = await _w.BeginAsync(_w.Circle, "OBSERVE");
        (await RefusedAsync(() => ExecAsync(c, tx, $"UPDATE budget.observation SET selected_for_return = true WHERE observation_id = {obs}")))
            .MessageText.Should().Be("DB-15");
        await c.DisposeAsync();

        // Inside a RETURN: once each.
        (c, tx) = await _w.BeginAsync(_w.Circle, "RETURN");
        await ExecAsync(c, tx, $"UPDATE budget.observation SET selected_for_return = true, returned_by_movement_id = {movement} WHERE observation_id = {obs}");
        await tx.CommitAsync();
        await c.DisposeAsync();
        (c, tx) = await _w.BeginAsync(_w.Circle, "RETURN");
        (await RefusedAsync(() => ExecAsync(c, tx, $"UPDATE budget.observation SET selected_for_return = false WHERE observation_id = {obs}")))
            .MessageText.Should().Be("DB-15");
        await c.DisposeAsync();

        // The reply, once; replied_by from the context.
        (c, tx) = await _w.BeginAsync(_w.DivisionA, "REPLY");
        await ExecAsync(c, tx, $"UPDATE budget.observation SET reply_text = 'Justified' WHERE observation_id = {obs}");
        (await ExecScalarAsync(c, tx, $"SELECT count(*) FROM budget.observation WHERE observation_id = {obs} " +
                                      $"AND replied_by = '{_w.DivisionA.Actor!.EmployeeId}' AND replied_at IS NOT NULL")).Should().Be(1);
        await tx.CommitAsync();
        await c.DisposeAsync();
        (c, tx) = await _w.BeginAsync(_w.DivisionA, "REPLY");
        (await RefusedAsync(() => ExecAsync(c, tx, $"UPDATE budget.observation SET reply_text = 'Again' WHERE observation_id = {obs}")))
            .MessageText.Should().Be("DB-15");
        await c.DisposeAsync();
    }

    [Fact]
    public async Task DB01_WORKS_proposal_with_a_Circle_origin_is_refused()
    {
        var (c, tx) = await _w.BeginAsync(_w.ItCell);
        await using var _ = c;
        (await RefusedAsync(() => ExecAsync(c, tx,
                "INSERT INTO budget.proposal (cycle_id, category, origin_office_id, state_code, holder_office_id) " +
                $"VALUES ({_w.CycleId}, 'WORKS', {_w.CircleOfficeId}, 'S-01', {_w.CircleOfficeId})")))
            .MessageText.Should().Be("DB-01");
    }

    [Fact]
    public async Task DB04_WORK_line_whose_work_is_of_another_Division_is_refused()
    {
        var (c, tx) = await _w.BeginAsync(_w.DivisionA);
        await using var _ = c;
        (await RefusedAsync(() => ExecScalarAsync(c, tx, InsertWorkLine(_w.WorkBId, 1)))).MessageText.Should().Be("DB-04");
    }

    [Fact]
    public async Task DB03_line_against_a_code_marked_not_for_use_is_refused_with_M05()
    {
        var (c, tx) = await _w.BeginAsync(_w.ItCell);
        await using (c)
        {
            await ExecAsync(c, tx,
                $"UPDATE budget.budget_head_master SET not_for_use = true, not_for_use_ref = 'EO/1', not_for_use_at = now(), " +
                $"not_for_use_by = '{_w.ItCell.Actor!.EmployeeId}' WHERE account_code_id = {_w.AccountCodeId}");
            await tx.CommitAsync();
        }
        var (c2, tx2) = await _w.BeginAsync(_w.DivisionA);
        await using var _ = c2;
        (await RefusedAsync(() => ExecScalarAsync(c2, tx2, InsertWorkLine(_w.WorkAId, 1)))).MessageText.Should().Be("M-05");
    }

    [Fact]
    public async Task M25_duplicate_WORK_line_is_refused_by_the_partial_unique_key_and_maps_to_M25()
    {
        var (c, tx) = await _w.BeginAsync(_w.DivisionA);
        await using var _ = c;
        await ExecScalarAsync(c, tx, InsertWorkLine(_w.WorkAId, 1));
        var ex = await RefusedAsync(() => ExecScalarAsync(c, tx, InsertWorkLine(_w.WorkAId, 2)));
        ex.SqlState.Should().Be(PostgresErrorCodes.UniqueViolation);
        BudgetMessages.FromPostgres(ex)!.Value.MessageId.Should().Be("M-25");
    }
}
