using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Npgsql;
namespace BBMB.BudgetService.Tests.Migrations;
/// <summary>
/// Database-layer tests for BudgetP2AccountCodesAndAssignment (design §4.7, §11.4 P2 row 2; DDD T-01 to T-07, T-21,
/// DB-01 to DB-03; DV-01, DV-20). The connection is the table owner, so every refusal is by trigger, CHECK or FK.
/// </summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class BudgetP2AccountCodesAndAssignmentTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private const string PreviousMigration = "BudgetP2TsCeilingLock";
    private const string ThisMigration = "BudgetP2AccountCodesAndAssignment";
    private SpineWorld _w = default!;
    public async Task InitializeAsync() => _w = await SpineWorld.CreateAsync(fixture, $"p2aca_{Guid.NewGuid():N}"[..22]);
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<long> RunAsync(FakeBudgetActorSource actor, string sql, string action = "TEST")
    {
        var (c, tx) = await _w.BeginAsync(actor, action);
        await using var _ = c;
        await using var cmd = new NpgsqlCommand(sql, c, tx);
        var v = await cmd.ExecuteScalarAsync();
        await tx.CommitAsync();
        return v is null or DBNull ? 0 : Convert.ToInt64(v);
    }

    private async Task<PostgresException> RefusedAsync(FakeBudgetActorSource actor, string sql) =>
        (await FluentActions.Invoking(() => RunAsync(actor, sql)).Should().ThrowAsync<PostgresException>()).Which;

    private async Task MarkNotForUseAsync() =>
        await RunAsync(_w.ItCell,
            "UPDATE budget.budget_head_master SET not_for_use = true, not_for_use_ref = 'EO/1', not_for_use_at = now(), " +
            $"not_for_use_by = '{_w.ItCell.Actor!.EmployeeId}' WHERE account_code_id = {_w.AccountCodeId}");

    private async Task<long> SecondCodeAsync()
    {
        var seg = await TestMasters.InsertSegmentsAsync(_w.ConnectionString);
        return await RunAsync(_w.ItCell,
            "INSERT INTO budget.budget_head_master (major_acc_code, sub_major_acc_code, minor_acc_code, sub_minor_acc_code, " +
            "detail_acc_code, object_acc_code, head_desc, party_emp_required, origin, modification_date, user_code) " +
            $"VALUES ('{seg.Major}', '{seg.SubMajor}', '{seg.Minor}', '{seg.SubMinor}', '{seg.Detail}', '{seg.Object}', " +
            "'Second head', 'N', 'LEGACY', now(), 'T') RETURNING account_code_id");
    }

    private Task<long> DnaProposalAsync(long officeId) =>
        RunAsync(_w.ItCell,
            "INSERT INTO budget.proposal (cycle_id, category, origin_office_id, state_code, holder_office_id) " +
            $"VALUES ({_w.CycleId}, 'DNA', {officeId}, 'S-01', {officeId}) RETURNING proposal_id");

    private static string HeadLine(long proposalId, long code, int serial = 1) =>
        "INSERT INTO budget.demand_line (proposal_id, line_type, serial_no, account_code_id) " +
        $"VALUES ({proposalId}, 'HEAD', {serial}, {code}) RETURNING demand_line_id";

    // ------------------------------------------------------------------ segment masters (DV-01, DV-20)

    [Fact]
    public async Task DV20_segment_nomenclature_update_is_refused()
    {
        var e = await RefusedAsync(_w.ItCell, $"UPDATE budget.fa_major_head SET major_acc_desc = 'Renamed' WHERE major_acc_code = '{_w.Segments.Major}'");
        e.MessageText.Should().Be("FORBIDDEN");
        (await RefusedAsync(_w.ItCell, $"UPDATE budget.fa_object_head SET object_acc_desc = 'X' WHERE object_acc_code = '{_w.Segments.Object}'"))
            .MessageText.Should().Be("FORBIDDEN");
    }

    [Fact]
    public async Task DV01_a_segment_code_not_of_the_DDD_form_is_refused_by_CHECK()
    {
        (await RefusedAsync(_w.ItCell,
                "INSERT INTO budget.fa_minor_head (minor_acc_code, minor_acc_desc, modification_date, user_code) VALUES ('1A2', 'Bad', now(), 'T')"))
            .ConstraintName.Should().Be("ck_fa_minor_head_code");
    }

    [Fact]
    public async Task T07_a_code_whose_sub_major_pair_is_not_in_T02_is_refused_by_the_composite_FK()
    {
        var seg = _w.Segments;
        var e = await RefusedAsync(_w.ItCell,
            "INSERT INTO budget.budget_head_master (major_acc_code, sub_major_acc_code, minor_acc_code, sub_minor_acc_code, " +
            "detail_acc_code, object_acc_code, head_desc, party_emp_required, origin, modification_date, user_code) " +
            $"VALUES ('{seg.Major}', '99', '{seg.Minor}', '{seg.SubMinor}', '{seg.Detail}', '{seg.Object}', 'Orphan', 'N', 'LEGACY', now(), 'T')");
        e.SqlState.Should().Be("23503");
        e.ConstraintName.Should().Be("fk_budget_head_master_sub_major");
    }

    // ------------------------------------------------------------------ T-07 trg_head_master_protect (DB-03)

    [Fact]
    public async Task DB03_head_desc_update_is_refused()
    {
        (await RefusedAsync(_w.ItCell, $"UPDATE budget.budget_head_master SET head_desc = 'X' WHERE account_code_id = {_w.AccountCodeId}"))
            .MessageText.Should().Be("DB-03");
    }

    [Fact]
    public async Task DB03_not_for_use_cannot_be_removed()
    {
        await MarkNotForUseAsync();
        (await RefusedAsync(_w.ItCell,
                "UPDATE budget.budget_head_master SET not_for_use = false, not_for_use_ref = NULL, not_for_use_at = NULL, not_for_use_by = NULL " +
                $"WHERE account_code_id = {_w.AccountCodeId}"))
            .MessageText.Should().Be("DB-03");
    }

    [Fact]
    public async Task BUD268_party_emp_required_changes_under_the_IT_Cell_only()
    {
        (await RefusedAsync(_w.DivisionA, $"UPDATE budget.budget_head_master SET party_emp_required = 'Y' WHERE account_code_id = {_w.AccountCodeId}"))
            .MessageText.Should().Be("DB-03");
        await RunAsync(_w.ItCell, $"UPDATE budget.budget_head_master SET party_emp_required = 'Y' WHERE account_code_id = {_w.AccountCodeId}");
    }

    [Fact]
    public async Task BUD308_marking_a_code_used_by_a_demand_line_is_refused_M33()
    {
        var p = await DnaProposalAsync(_w.DivisionAOfficeId);
        await RunAsync(_w.DivisionA, HeadLine(p, _w.AccountCodeId));
        var e = await FluentActions.Invoking(MarkNotForUseAsync).Should().ThrowAsync<PostgresException>();
        e.Which.MessageText.Should().Be("M-33");
    }

    [Fact]
    public async Task DB03_a_work_referencing_a_code_marked_not_for_use_is_refused_on_insert()
    {
        await MarkNotForUseAsync();
        var seg = _w.Segments;
        (await RefusedAsync(_w.DivisionA,
                "INSERT INTO budget.work (work_code, division_id, work_desc, user_code, work_class, entered_via, major_acc_code, sub_major_acc_code, " +
                "minor_acc_code, sub_minor_acc_code, detail_acc_code, object_acc_code, original_budget, revised_budget, proposed_budget, remarks) " +
                $"VALUES (588888, {_w.HierarchyA.DivisionId}, 'New', 'T', 'AA', 'DEMAND_ENTRY', '{seg.Major}', '{seg.SubMajor}', '{seg.Minor}', " +
                $"'{seg.SubMinor}', '{seg.Detail}', '{seg.Object}', '', '', '', '')"))
            .MessageText.Should().Be("M-05");
        // An existing work keeping its classification is not a new reference.
        await RunAsync(_w.DivisionA, $"UPDATE budget.work SET work_desc = 'Renamed' WHERE id = {_w.WorkAId}");
    }

    // ------------------------------------------------------------------ T-21 and DB-02

    [Fact]
    public async Task DB02_head_line_for_an_unassigned_head_is_refused_M13()
    {
        var other = await SecondCodeAsync();
        var p = await DnaProposalAsync(_w.DivisionAOfficeId);
        (await RefusedAsync(_w.DivisionA, HeadLine(p, other))).MessageText.Should().Be("M-13");
        await RunAsync(_w.DivisionA, HeadLine(p, _w.AccountCodeId)); // the world's code is assigned (R04_LOAD)
    }

    [Fact]
    public async Task DB01_assignment_to_a_FINANCE_office_is_refused()
    {
        var other = await SecondCodeAsync();
        (await RefusedAsync(_w.ItCell,
                $"INSERT INTO budget.account_head_assignment (office_id, account_code_id, source) VALUES ({_w.FinanceOfficeId}, {other}, 'AFA')"))
            .MessageText.Should().Be("DB-01");
    }

    [Fact]
    public async Task DB03_assignment_of_a_code_marked_not_for_use_is_refused_M05()
    {
        var other = await SecondCodeAsync();
        await RunAsync(_w.ItCell,
            "UPDATE budget.budget_head_master SET not_for_use = true, not_for_use_ref = 'EO/2', not_for_use_at = now(), " +
            $"not_for_use_by = '{_w.ItCell.Actor!.EmployeeId}' WHERE account_code_id = {other}");
        (await RefusedAsync(_w.ItCell,
                $"INSERT INTO budget.account_head_assignment (office_id, account_code_id, source) VALUES ({_w.DivisionAOfficeId}, {other}, 'AFA')"))
            .MessageText.Should().Be("M-05");
    }

    [Fact]
    public async Task BUD170_removal_with_a_live_line_is_refused_M15_and_without_one_is_marked_with_the_actor()
    {
        var p = await DnaProposalAsync(_w.DivisionAOfficeId);
        var line = await RunAsync(_w.DivisionA, HeadLine(p, _w.AccountCodeId));
        var removal = "UPDATE budget.account_head_assignment SET removed_at = now(), removed_by = gen_random_uuid() " +
                      $"WHERE office_id = {_w.DivisionAOfficeId} AND account_code_id = {_w.AccountCodeId}";
        (await RefusedAsync(_w.ItCell, removal)).MessageText.Should().Be("M-15");

        await RunAsync(_w.DivisionA, $"UPDATE budget.demand_line SET is_deleted = true WHERE demand_line_id = {line}");
        await RunAsync(_w.ItCell, removal);
        (await RunAsync(_w.ItCell,
                "SELECT count(*) FROM budget.account_head_assignment " +
                $"WHERE office_id = {_w.DivisionAOfficeId} AND account_code_id = {_w.AccountCodeId} " +
                $"AND removed_by = '{_w.ItCell.Actor!.EmployeeId}'"))
            .Should().Be(1, "removed_by comes from the actor context, never from the statement");
        (await RefusedAsync(_w.ItCell,
                $"UPDATE budget.account_head_assignment SET removed_at = NULL WHERE office_id = {_w.DivisionAOfficeId} AND account_code_id = {_w.AccountCodeId}"))
            .MessageText.Should().Be("DB-02");
    }

    [Fact]
    public async Task DB02_assignment_rows_are_never_deleted_and_the_live_pair_is_unique()
    {
        (await RefusedAsync(_w.ItCell, "DELETE FROM budget.account_head_assignment")).MessageText.Should().Be("FORBIDDEN");
        (await RefusedAsync(_w.ItCell,
                $"INSERT INTO budget.account_head_assignment (office_id, account_code_id, source) VALUES ({_w.DivisionAOfficeId}, {_w.AccountCodeId}, 'AFA')"))
            .ConstraintName.Should().Be("ux_account_head_assignment_live");
    }

    [Fact]
    public async Task V01_and_V07_show_the_code_with_its_nomenclatures_flag_and_mark()
    {
        (await RunAsync(_w.ItCell,
                $"SELECT count(*) FROM budget.v_manual_of_accounts_codes WHERE account_code_id = {_w.AccountCodeId} " +
                "AND major_acc_desc IS NOT NULL AND object_acc_desc IS NOT NULL AND origin = 'LEGACY' AND NOT not_for_use"))
            .Should().Be(1);
        await MarkNotForUseAsync();
        (await RunAsync(_w.ItCell,
                $"SELECT count(*) FROM budget.v_fa_account_code WHERE account_code_id = {_w.AccountCodeId} AND not_for_use AND party_emp_required = 'N'"))
            .Should().Be(1);
    }

    // ------------------------------------------------------------------ guard

    [Fact]
    public async Task Guard_aborts_on_a_segment_code_not_of_the_DDD_form_before_any_change()
    {
        var cs = await fixture.CreateFreshDatabaseAsync($"p2aca_guard_{Guid.NewGuid():N}"[..24]);
        await fixture.MigrateToAsync(cs, PreviousMigration);
        await using (var c = new NpgsqlConnection(cs))
        {
            await c.OpenAsync();
            await using var cmd = new NpgsqlCommand(
                "INSERT INTO budget.fa_major_head (major_acc_code, major_acc_desc, modification_date, user_code) VALUES ('12A4', 'Bad code', now(), 'T')", c);
            await cmd.ExecuteNonQueryAsync();
        }
        var act = () => fixture.MigrateToAsync(cs, ThisMigration);
        var e = (await act.Should().ThrowAsync<PostgresException>()).Which;
        e.MessageText.Should().Contain("fa_major_head.major_acc_code").And.Contain("1 row(s)");
        await using var c2 = new NpgsqlConnection(cs);
        await c2.OpenAsync();
        await using var check = new NpgsqlCommand("SELECT to_regclass('budget.account_head_assignment') IS NULL", c2);
        ((bool)(await check.ExecuteScalarAsync())!).Should().BeTrue("the migration rolled back as a whole");
    }
}
