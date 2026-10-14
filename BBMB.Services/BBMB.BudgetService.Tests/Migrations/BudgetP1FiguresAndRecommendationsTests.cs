using System.Text.Json;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BBMB.BudgetService.Tests.Migrations;

/// <summary>Design §5.3 (T-35 to T-38), §5.4 (V-03, V-04 v1); DDD DB-01, DB-14, DB-16, DB-27, BUD-025.</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class BudgetP1FiguresAndRecommendationsTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;

    public async Task InitializeAsync() => _w = await SpineWorld.CreateAsync(fixture, $"fig_{Guid.NewGuid():N}"[..20]);

    public Task DisposeAsync() => Task.CompletedTask;

    private static async Task<long> ScalarAsync(NpgsqlConnection c, NpgsqlTransaction tx, string sql)
    {
        await using var cmd = new NpgsqlCommand(sql, c, tx);
        var v = await cmd.ExecuteScalarAsync();
        return v is null or DBNull ? 0 : Convert.ToInt64(v);
    }

    private async Task<long> RunAsync(FakeBudgetActorSource actor, string sql, string action = "TEST")
    {
        var (c, tx) = await _w.BeginAsync(actor, action);
        await using var _ = c;
        var v = await ScalarAsync(c, tx, sql);
        await tx.CommitAsync();
        return v;
    }

    private async Task<PostgresException> RefusedAsync(FakeBudgetActorSource actor, string sql)
    {
        var (c, tx) = await _w.BeginAsync(actor);
        await using var _ = c;
        var act = () => ScalarAsync(c, tx, sql);
        return (await act.Should().ThrowAsync<PostgresException>()).Which;
    }

    private string OpeningWork(long cycleId, decimal amount = 1000) =>
        "INSERT INTO budget.opening_figure (cycle_id, subject_type, work_id, figure_kind, amount) " +
        $"VALUES ({cycleId}, 'WORK', {_w.WorkAId}, 'OBE_CURRENT', {amount}) RETURNING opening_figure_id";

    [Fact]
    public async Task DB16_opening_figure_accepted_in_the_first_cycle_only()
    {
        (await RunAsync(_w.DivisionA, OpeningWork(_w.CycleId))).Should().BeGreaterThan(0);

        var second = await RunAsync(_w.ItCell,
            "INSERT INTO budget.fa_financial_years (fin_year_closing, start_date, end_date) VALUES (2028, '2027-04-01', '2028-03-31'); " +
            "INSERT INTO budget.budget_cycle (current_fin_year, next_fin_year, is_first_cycle, status) VALUES (2027, 2028, false, 'OPEN') " +
            "RETURNING cycle_id");
        (await RefusedAsync(_w.DivisionA, OpeningWork(second))).MessageText.Should().Be("DB-16");
    }

    [Fact]
    public async Task DB16_opening_figure_fixed_once_the_origin_proposal_is_first_submitted()
    {
        var id = await RunAsync(_w.DivisionA, OpeningWork(_w.CycleId));
        await RunAsync(_w.ItCell, $"UPDATE budget.proposal SET first_submitted_at = now() WHERE proposal_id = {_w.WorksProposalAId}");

        (await RefusedAsync(_w.DivisionA, $"UPDATE budget.opening_figure SET amount = 2000 WHERE opening_figure_id = {id}"))
            .MessageText.Should().Be("DB-16");
    }

    [Fact]
    public async Task BUD025_T35_is_append_only_and_a_keyed_figure_is_never_replaced_by_a_drawn_one()
    {
        string Exp(decimal amount, string source, string asAt) =>
            "INSERT INTO budget.expenditure_figure (subject_type, work_id, nature, fin_year, period, amount, source, as_at) " +
            $"VALUES ('WORK', {_w.WorkAId}, 'EXPENDITURE', 2026, 'UP_TO_DATE', {amount}, '{source}', '{asAt}') RETURNING expenditure_figure_id";

        var first = await RunAsync(_w.DivisionA, Exp(100, "KEYED", "2026-01-01"));
        await RunAsync(_w.DivisionA, Exp(200.55m, "DRAWN", "2026-02-01"));
        (await RefusedAsync(_w.DivisionA, $"UPDATE budget.expenditure_figure SET amount = 1 WHERE expenditure_figure_id = {first}"))
            .MessageText.Should().Be("FORBIDDEN");
        (await RefusedAsync(_w.DivisionA, Exp(100.50m, "KEYED", "2026-03-01"))).SqlState
            .Should().Be(PostgresErrorCodes.CheckViolation, "a keyed figure is whole rupees");

        await RunAsync(_w.DivisionA,
            "INSERT INTO budget.demand_line (proposal_id, line_type, serial_no, work_id, account_code_id, rbe_amount, obe_next_amount) " +
            $"VALUES ({_w.WorksProposalAId}, 'WORK', 1, {_w.WorkAId}, {_w.AccountCodeId}, 500, 50) RETURNING demand_line_id");

        await using var ctx = _w.NewContext();
        (await ctx.DemandLineDisplay.SingleAsync(r => r.WorkId == _w.WorkAId)).ExpenditureToDate
            .Should().Be(100m, "the later drawn figure does not replace the keyed one");

        await RunAsync(_w.DivisionA, Exp(150, "KEYED", "2026-03-01"));
        await using var ctx2 = _w.NewContext();
        (await ctx2.DemandLineDisplay.SingleAsync(r => r.WorkId == _w.WorkAId)).ExpenditureToDate
            .Should().Be(150m, "a later keyed value is a new row and is current");
    }

    [Fact]
    public async Task DB14_recommendation_change_writes_a_version_with_C_columns_and_an_audit_row()
    {
        var afa = new FakeBudgetActorSource(_w.ItCell.Actor! with { OfficeId = _w.FinanceOfficeId, OfficeLevel = "FINANCE", RoleCode = "AFA_BUDGET" });
        var id = await RunAsync(afa,
            "INSERT INTO budget.ce_recommendation (cycle_id, category, ce_office_id, account_code_id, rbe_recommended, obe_next_recommended, concurrence_ref) " +
            $"VALUES ({_w.CycleId}, 'DNA', {_w.CeOfficeId}, {_w.AccountCodeId}, 1000, 2000, 'FIN/1') RETURNING recommendation_id");
        await RunAsync(afa, $"UPDATE budget.ce_recommendation SET rbe_recommended = 1500 WHERE recommendation_id = {id}");

        var (c, tx) = await _w.BeginAsync(_w.ItCell);
        await using var _ = c;
        (await ScalarAsync(c, tx, $"SELECT count(*) FROM budget.recommendation_version WHERE recommendation_id = {id}")).Should().Be(2);
        (await ScalarAsync(c, tx,
                $"SELECT count(*) FROM budget.recommendation_version WHERE recommendation_id = {id} AND version_no = 2 " +
                $"AND rbe_recommended = 1500 AND created_by = '{afa.Actor!.EmployeeId}' AND created_charge_id = '{afa.Actor.ChargeId}'"))
            .Should().Be(1);
        (await ScalarAsync(c, tx, $"SELECT count(*) FROM budget.audit_log WHERE table_name = 'recommendation_version' " +
                                  $"AND operation = 'I' AND record_key = '{id}|2'")).Should().Be(1);
    }

    [Fact]
    public async Task DB01_recommendation_for_a_Circle_is_refused_and_concurrence_is_required_with_a_figure()
    {
        (await RefusedAsync(_w.ItCell,
                "INSERT INTO budget.ce_recommendation (cycle_id, category, ce_office_id, account_code_id, concurrence_ref) " +
                $"VALUES ({_w.CycleId}, 'DNA', {_w.CircleOfficeId}, {_w.AccountCodeId}, 'X')"))
            .MessageText.Should().Be("DB-01");
        (await RefusedAsync(_w.ItCell,
                "INSERT INTO budget.ce_recommendation (cycle_id, category, ce_office_id, account_code_id, rbe_recommended) " +
                $"VALUES ({_w.CycleId}, 'DNA', {_w.CeOfficeId}, {_w.AccountCodeId}, 10)"))
            .SqlState.Should().Be(PostgresErrorCodes.CheckViolation);
    }

    [Fact]
    public async Task V04_returns_a_row_per_line_with_the_T31_figure_at_each_level()
    {
        var line = await RunAsync(_w.DivisionA,
            "INSERT INTO budget.demand_line (proposal_id, line_type, serial_no, work_id, account_code_id, rbe_amount, obe_next_amount) " +
            $"VALUES ({_w.WorksProposalAId}, 'WORK', 1, {_w.WorkAId}, {_w.AccountCodeId}, 500, 50) RETURNING demand_line_id");

        // Move to S-03 at the Circle: SUBMIT (T01) then OPEN (T05).
        var (c, tx) = await _w.BeginAsync(_w.Circle, "OPEN");
        await using (c)
        {
            foreach (var sql in new[]
                     {
                         "INSERT INTO budget.proposal_movement (proposal_id, from_state, action_code, to_state, from_office_id, to_office_id, role_code, client_ip) " +
                         $"VALUES ({_w.WorksProposalAId}, 'S-01', 'SUBMIT', 'S-02', {_w.DivisionAOfficeId}, {_w.CircleOfficeId}, 'DDO', '192.0.2.1'), " +
                         $"({_w.WorksProposalAId}, 'S-02', 'OPEN', 'S-03', {_w.CircleOfficeId}, {_w.CircleOfficeId}, 'SE', '192.0.2.1')",
                         $"UPDATE budget.proposal SET state_code = 'S-02', holder_office_id = {_w.CircleOfficeId} WHERE proposal_id = {_w.WorksProposalAId}",
                         $"UPDATE budget.proposal SET state_code = 'S-03' WHERE proposal_id = {_w.WorksProposalAId}",
                     })
            {
                await ScalarAsync(c, tx, sql);
            }
            await tx.CommitAsync();
        }
        await RunAsync(_w.Circle, $"UPDATE budget.demand_line SET rbe_amount = 400 WHERE demand_line_id = {line}", "ALTER");

        await using var ctx = _w.NewContext();
        var row = await ctx.DemandLineDisplay.SingleAsync(r => r.DemandLineId == line);
        row.RbeAmount.Should().Be(400m);
        row.WorkClass.Should().Be("AA");
        row.CompleteCode.Should().Be(_w.Segments.Complete);

        var levels = JsonSerializer.Deserialize<List<JsonElement>>(row.LevelFigures)!;
        levels.Select(l => (l.GetProperty("level_office_id").GetInt64(), l.GetProperty("figure").GetString(), l.GetProperty("value").GetDecimal()))
            .Should().BeEquivalentTo(new[]
            {
                (_w.DivisionAOfficeId, "RBE", 500m), (_w.DivisionAOfficeId, "OBE_NEXT", 50m), (_w.CircleOfficeId, "RBE", 400m),
            });
    }

    [Fact]
    public async Task V03_sums_head_wise_demand_per_CE_and_account_code()
    {
        var dnaDiv = await RunAsync(_w.ItCell,
            "INSERT INTO budget.proposal (cycle_id, category, origin_office_id, state_code, holder_office_id) " +
            $"VALUES ({_w.CycleId}, 'DNA', {_w.DivisionAOfficeId}, 'S-01', {_w.DivisionAOfficeId}) RETURNING proposal_id");
        var dnaCircle = await RunAsync(_w.ItCell,
            "INSERT INTO budget.proposal (cycle_id, category, origin_office_id, state_code, holder_office_id) " +
            $"VALUES ({_w.CycleId}, 'DNA', {_w.CircleOfficeId}, 'S-01', {_w.CircleOfficeId}) RETURNING proposal_id");
        string Head(long p, int serial, decimal rbe, decimal obe) =>
            "INSERT INTO budget.demand_line (proposal_id, line_type, serial_no, account_code_id, rbe_amount, obe_next_amount) " +
            $"VALUES ({p}, 'HEAD', {serial}, {_w.AccountCodeId}, {rbe}, {obe}) RETURNING demand_line_id";
        await RunAsync(_w.DivisionA, Head(dnaDiv, 1, 100, 10));
        await RunAsync(_w.Circle, Head(dnaCircle, 1, 250, 25));

        await using var ctx = _w.NewContext();
        var row = await ctx.DemandByCe.SingleAsync(r => r.CycleId == _w.CycleId && r.Category == "DNA");
        row.CeOfficeId.Should().Be(_w.CeOfficeId);
        row.AccountCodeId.Should().Be(_w.AccountCodeId);
        row.RbeTotal.Should().Be(350m);
        row.ObeNextTotal.Should().Be(35m);
        row.LineCount.Should().Be(2);
        row.OfficeCount.Should().Be(2);
    }
}
