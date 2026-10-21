using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BBMB.BudgetService.Tests.Migrations;

/// <summary>Design §5.5 rows j and l, §6.2, §11.1 rule 9, §11.4 P1 row 4; DDD DV-07, L-02.</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class BudgetP1StateTransitionSeedTests(BudgetDatabaseFixture fixture)
{
    [Fact]
    public async Task BUD235_seed_holds_the_design_6_2_rows()
    {
        await using var ctx = fixture.NewBudgetContext();
        var rows = await ctx.StateTransitions.AsNoTracking().ToListAsync();
        var triples = rows.Select(r => (r.FromState, r.ActionCode, r.ToState)).ToHashSet();

        rows.Should().HaveCount(37);
        rows.Where(r => r.FromState == "S-01" && r.ActionCode == "SUBMIT").Select(r => r.ToState)
            .Should().BeEquivalentTo("S-02", "S-05", "S-07");
        triples.Should().Contain(("S-02", "RECALL", "S-04"), "T04b");
        triples.Should().Contain(("S-05", "RECALL", "S-04"), "T09c");
        triples.Should().Contain(("S-07", "RECALL", "S-04"), "T13c");
        triples.Should().Contain(("S-09", "RECALL", "S-08"), "T17");
        triples.Should().Contain(("S-11", "REOPEN", "S-10"), "T21");
        triples.Should().NotContain(("S-11", "PLACE_FB", "S-14"), "the first pass's T25b is dropped");

        rows.Where(r => r.RemarkRequired).Select(r => r.ActionCode).Distinct()
            .Should().BeEquivalentTo("RETURN", "SEND_BACK", "REFER_BACK", "REOPEN");
    }

    [Fact]
    public async Task L02_every_seeded_transition_has_a_T51_insert_row_under_the_migration_actor()
    {
        await using var ctx = fixture.NewBudgetContext();
        var keys = await ctx.StateTransitions.AsNoTracking()
            .Select(r => r.FromState + "|" + r.ActionCode + "|" + r.ToState).ToListAsync();
        var audit = await ctx.AuditLog.AsNoTracking()
            .Where(a => a.TableName == "state_transition" && a.Operation == "I").ToListAsync();

        audit.Select(a => a.RecordKey).Should().BeEquivalentTo(keys);
        audit.Should().OnlyContain(a => a.ActionCode == "MIGRATION:BudgetP1StateTransitionSeed"
                                        && a.ActorRoleCode == "MIGRATION"
                                        && a.ActorName == BudgetDatabaseFixture.MigrationEmployeeName);
    }

    [Fact]
    public async Task DV07_a_movement_whose_triple_is_not_in_T16_is_refused()
    {
        var w = await SpineWorld.CreateAsync(fixture, $"dv07_{Guid.NewGuid():N}"[..20]);
        var (c, tx) = await w.BeginAsync(w.DivisionA, "SUBMIT");
        await using var _ = c;
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO budget.proposal_movement (proposal_id, from_state, action_code, to_state, from_office_id, to_office_id, role_code, client_ip) " +
            $"VALUES ({w.WorksProposalAId}, 'S-01', 'SUBMIT', 'S-03', {w.DivisionAOfficeId}, {w.CircleOfficeId}, 'DDO', '192.0.2.1')", c, tx);
        var act = () => cmd.ExecuteNonQueryAsync();
        var ex = (await act.Should().ThrowAsync<PostgresException>()).Which;
        ex.SqlState.Should().Be(PostgresErrorCodes.ForeignKeyViolation);
        ex.ConstraintName.Should().Be("fk_proposal_movement_state_transition");
    }

    [Fact]
    public async Task DB12_a_RETURN_two_levels_down_is_refused()
    {
        // DV-07 / DB-12: S-06 RETURN S-03 must go to the office immediately below the returning CE, not to a Division.
        var w = await SpineWorld.CreateAsync(fixture, $"db12_{Guid.NewGuid():N}"[..20]);
        var (c, tx) = await w.BeginAsync(w.Circle, "RETURN");
        await using var _ = c;
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO budget.proposal_movement (proposal_id, from_state, action_code, to_state, from_office_id, to_office_id, remark, role_code, client_ip) " +
            $"VALUES ({w.WorksProposalAId}, 'S-06', 'RETURN', 'S-03', {w.CeOfficeId}, {w.DivisionAOfficeId}, 'see remarks', 'CE', '192.0.2.1')", c, tx);
        var act = () => cmd.ExecuteNonQueryAsync();
        (await act.Should().ThrowAsync<PostgresException>()).Which.MessageText.Should().Be("DB-12");
    }

    [Fact]
    public async Task DB12_a_RETURN_without_a_remark_is_refused_with_M12()
    {
        var w = await SpineWorld.CreateAsync(fixture, $"m12_{Guid.NewGuid():N}"[..20]);
        var (c, tx) = await w.BeginAsync(w.Circle, "RETURN");
        await using var _ = c;
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO budget.proposal_movement (proposal_id, from_state, action_code, to_state, from_office_id, to_office_id, remark, role_code, client_ip) " +
            $"VALUES ({w.WorksProposalAId}, 'S-03', 'RETURN', 'S-04', {w.CircleOfficeId}, {w.DivisionAOfficeId}, '  ', 'SE', '192.0.2.1')", c, tx);
        var act = () => cmd.ExecuteNonQueryAsync();
        (await act.Should().ThrowAsync<PostgresException>()).Which.MessageText.Should().Be("M-12");
    }

    [Fact]
    public async Task Rule9_p1_state_model_unset_aborts_only_the_seed_migration()
    {
        var cs = await fixture.CreateFreshDatabaseAsync("seed_nogate", BudgetDatabaseFixture.AllGates.Where(g => g != "p1_state_model"));
        var act = () => fixture.MigrateToAsync(cs, null);
        (await act.Should().ThrowAsync<PostgresException>()).Which.MessageText.Should().StartWith("gate p1_state_model not set");

        await using var ctx = BudgetDatabaseFixture.NewBudgetContext(cs);
        var applied = await ctx.Database.GetAppliedMigrationsAsync();
        applied.Should().Contain(m => m.EndsWith("_BudgetP1RupeesAndMoneyColumns"));
        applied.Should().Contain(m => m.EndsWith("_BudgetP1ReferenceStateAndAudit"));
        applied.Should().Contain(m => m.EndsWith("_BudgetP1ProposalSpineMasters"));
        applied.Should().Contain(m => m.EndsWith("_BudgetP1ProposalSpine"));
        applied.Should().Contain(m => m.EndsWith("_BudgetP1FiguresAndRecommendations"));
        applied.Should().NotContain(m => m.EndsWith("_BudgetP1StateTransitionSeed"));

        var exists = await ctx.Database.SqlQueryRaw<string>(
                "SELECT coalesce(to_regclass('budget.audit_log')::text, '') || ',' || coalesce(to_regproc('budget.fn_audit_row')::text, '') " +
                "|| ',' || coalesce(to_regclass('budget.state_transition')::text, '-') AS \"Value\"")
            .SingleAsync();
        exists.Should().Be("budget.audit_log,budget.fn_audit_row,-");
    }
}
