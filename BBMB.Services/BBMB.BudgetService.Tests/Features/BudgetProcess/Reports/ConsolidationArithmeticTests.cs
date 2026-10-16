using BBMB.BudgetService.Features.BudgetProcess.Proposals.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Reports.GetConsolidation;
using BBMB.BudgetService.Features.BudgetProcess.Reports.GetConsolidationComposition;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Npgsql;

namespace BBMB.BudgetService.Tests.Features.BudgetProcess.Reports;

/// <summary>
/// P7-03 (design §12 P6 exit, §8.8 F-18; RTM backend rows TC-BUD-BE-*). The consolidation-arithmetic test that
/// moved out of P6-01 when the reporting migration was split. It proves V-06-style consolidation: the
/// consolidated total of a hierarchy cell equals the sum of its contributing demand lines to the rupee, and
/// WORK and HEAD figures are totalled separately and once (BUD-311, BUD-177, BUD-178, BUD-266).
///
/// SCOPE AND A DISCLOSED GAP: the FULL V-06 v_consolidation view (P6-01b) aggregates allocation/provision
/// (T-40..T-42) and re-appropriation (T-46) and is still DEFERRED, so the ALLOCATED/frozen totals do not exist
/// yet. The reporting surface that DOES exist is budget.v_report_demand (P6-01), read by GetConsolidation /
/// GetConsolidationComposition, carrying the DEMANDED and DISTRIBUTED figures. This test exercises the
/// consolidation arithmetic over exactly that surface — the part of V-06 that is implemented — and asserts the
/// handler reports allocated figures as "not yet available" rather than fabricating them. The allocated-total
/// arithmetic is carried forward with P6-01b when it is un-deferred; see this item's evidence note.
///
/// Method names embed the BUD token so scripts/generate_rtm_bud.py auto-links each to its RTM test-case id.
/// </summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class ConsolidationArithmeticTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;
    private long _worksProposalB;

    public async Task InitializeAsync()
    {
        _w = await SpineWorld.CreateAsync(fixture, $"consol_{Guid.NewGuid():N}"[..22]);

        // A second WORKS proposal for Division B (same Circle -> CE as Division A), so grouping by division shows
        // two keys and grouping by CE rolls both up to one total.
        await using var ctx = _w.NewContext();
        await using (var tx = await new BBMB.BudgetService.Shared.Services.BudgetActors.BudgetActionScope(ctx, _w.ItCell).BeginAsync("OPEN_PROPOSALS"))
        {
            var p = new BudgetProposal
            {
                CycleId = _w.CycleId, Category = "WORKS", OriginOfficeId = _w.DivisionBOfficeId,
                HolderOfficeId = _w.DivisionBOfficeId, StateCode = "S-01",
            };
            ctx.BudgetProposals.Add(p);
            await ctx.SaveChangesAsync();
            await tx.CommitAsync();
            _worksProposalB = p.ProposalId;
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>Creates a fresh code-less work in the given Division (ux_demand_line_work requires one WORK line
    /// per work per proposal, so each WORK line needs its own work). Inserted directly with the segment masters
    /// SpineWorld seeded; work_code is null at entry (P4 §4.6).</summary>
    private async Task<int> NewWorkAsync(FakeBudgetActorSource actor, int divisionId)
    {
        var seg = _w.Segments;
        var (conn, tx) = await _w.BeginAsync(actor, "WORKS_ENTER");
        await using var _ = conn;
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO budget.work (work_code, division_id, work_desc, user_code, work_class, entered_via, pre_ddd, " +
            "major_acc_code, sub_major_acc_code, minor_acc_code, sub_minor_acc_code, detail_acc_code, object_acc_code, " +
            "original_budget, revised_budget, proposed_budget, remarks) " +
            $"VALUES (NULL, {divisionId}, 'Extra work', 'T', 'AA', 'LEGACY_OPENING', false, " +
            $"'{seg.Major}', '{seg.SubMajor}', '{seg.Minor}', '{seg.SubMinor}', '{seg.Detail}', '{seg.Object}', " +
            "'', '', '', '') RETURNING id", conn, tx);
        var id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        await tx.CommitAsync();
        return id;
    }

    /// <summary>Inserts a WORK demand line directly (as the P1 spine tests do), returning its demand_line_id.</summary>
    private async Task AddWorkLineAsync(FakeBudgetActorSource actor, long proposalId, int workId, int serial,
        decimal rbe, decimal obeNext)
    {
        var (conn, tx) = await _w.BeginAsync(actor, "DEMAND_SAVE");
        await using var _ = conn;
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO budget.demand_line (proposal_id, line_type, serial_no, work_id, account_code_id, rbe_amount, obe_next_amount) " +
            $"VALUES ({proposalId}, 'WORK', {serial}, {workId}, {_w.AccountCodeId}, {rbe}, {obeNext})", conn, tx);
        await cmd.ExecuteNonQueryAsync();
        await tx.CommitAsync();
    }

    /// <summary>Inserts a HEAD demand line directly. SpineWorld assigns the account code to the Division/Circle/CE
    /// offices (R04_LOAD), so the head-wise line's DB-02 precondition holds.</summary>
    private async Task AddHeadLineAsync(FakeBudgetActorSource actor, long proposalId, int serial, decimal rbe, decimal obeNext)
    {
        var (conn, tx) = await _w.BeginAsync(actor, "DEMAND_SAVE");
        await using var _ = conn;
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO budget.demand_line (proposal_id, line_type, serial_no, account_code_id, rbe_amount, obe_next_amount) " +
            $"VALUES ({proposalId}, 'HEAD', {serial}, {_w.AccountCodeId}, {rbe}, {obeNext})", conn, tx);
        await cmd.ExecuteNonQueryAsync();
        await tx.CommitAsync();
    }

    private async Task<FakeBudgetActorSource> FinanceActorAsync() =>
        await TestActor.CreateAsync(fixture, role: "AFA_BUDGET", officeId: _w.FinanceOfficeId,
            officeLevel: "FINANCE", name: "Finance Officer", connectionString: _w.ConnectionString);

    [Fact]
    public async Task BUD311_BUD177_ConsolidatedCeTotalEqualsSumOfItsWorkLines()
    {
        // Division A: three WORK lines (distinct works); Division B: two WORK lines. All under the one CE.
        var a2 = await NewWorkAsync(_w.DivisionA, _w.HierarchyA.DivisionId);
        var a3 = await NewWorkAsync(_w.DivisionA, _w.HierarchyA.DivisionId);
        var b2 = await NewWorkAsync(_w.DivisionB, _w.HierarchyB.DivisionId);
        await AddWorkLineAsync(_w.DivisionA, _w.WorksProposalAId, _w.WorkAId, 1, 100, 10);
        await AddWorkLineAsync(_w.DivisionA, _w.WorksProposalAId, a2, 2, 250, 25);
        await AddWorkLineAsync(_w.DivisionA, _w.WorksProposalAId, a3, 3, 75, 5);
        await AddWorkLineAsync(_w.DivisionB, _worksProposalB, _w.WorkBId, 1, 400, 40);
        await AddWorkLineAsync(_w.DivisionB, _worksProposalB, b2, 2, 300, 30);
        const decimal expectedRbe = 100 + 250 + 75 + 400 + 300; // 1125
        const decimal expectedObe = 10 + 25 + 5 + 40 + 30;      // 110

        var finance = await FinanceActorAsync();
        await using var ctx = _w.NewContext();
        var handler = new GetConsolidationHandler(ctx, finance, TestServices.OfficeScope(ctx));
        var result = await handler.Handle(new GetConsolidationQuery(_w.CycleId, "WORKS", null, "ce"), default);

        result.IsSucced.Should().BeTrue();
        var workRows = result.Data!.Rows.Where(r => r.LineClass == "WORK").ToList();
        workRows.Should().ContainSingle("every WORKS line rolls up to the one CE");
        workRows[0].DemandedRbe.Should().Be(expectedRbe);
        workRows[0].DemandedObeNext.Should().Be(expectedObe);

        // The allocated/frozen totals are NOT fabricated — they are reported as not yet available (P6-01b deferred).
        result.Data.AllocatedAvailable.Should().BeFalse();
        result.Data.AllocatedNote.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task BUD311_WorkAndHeadLinesAreTotalledSeparatelyAndOnce()
    {
        // Two WORK lines and one HEAD line in the same (DNA has HEAD; WORKS has WORK) — put both in Division A so
        // one CE cell carries both classes. A HEAD line goes in a DNA proposal of Division A.
        var a2 = await NewWorkAsync(_w.DivisionA, _w.HierarchyA.DivisionId);
        await AddWorkLineAsync(_w.DivisionA, _w.WorksProposalAId, _w.WorkAId, 1, 100, 10);
        await AddWorkLineAsync(_w.DivisionA, _w.WorksProposalAId, a2, 2, 200, 20);

        long dnaProposal;
        await using (var ctx = _w.NewContext())
        await using (var tx = await new BBMB.BudgetService.Shared.Services.BudgetActors.BudgetActionScope(ctx, _w.ItCell).BeginAsync("OPEN_PROPOSALS"))
        {
            var p = new BudgetProposal
            {
                CycleId = _w.CycleId, Category = "DNA", OriginOfficeId = _w.DivisionAOfficeId,
                HolderOfficeId = _w.DivisionAOfficeId, StateCode = "S-01",
            };
            ctx.BudgetProposals.Add(p);
            await ctx.SaveChangesAsync();
            await tx.CommitAsync();
            dnaProposal = p.ProposalId;
        }
        await AddHeadLineAsync(_w.DivisionA, dnaProposal, 1, 500, 50);

        var finance = await FinanceActorAsync();
        await using var ctx2 = _w.NewContext();
        var handler = new GetConsolidationHandler(ctx2, finance, TestServices.OfficeScope(ctx2));
        // No category filter so both WORKS and DNA lines are consolidated; grouped by CE.
        var result = await handler.Handle(new GetConsolidationQuery(_w.CycleId, null, null, "ce"), default);

        result.IsSucced.Should().BeTrue();
        var work = result.Data!.Rows.Single(r => r.LineClass == "WORK");
        var head = result.Data.Rows.Single(r => r.LineClass == "HEAD");
        work.DemandedRbe.Should().Be(300, "the two WORK lines are totalled once, apart from the HEAD line");
        head.DemandedRbe.Should().Be(500, "the HEAD line is totalled once, apart from the WORK lines");
    }

    [Fact]
    public async Task BUD178_CompositionLinesSumBackToTheConsolidatedCell()
    {
        // Grouped by DIVISION: a Division-origin line carries division_id (its ce_id/circle_id are null, since
        // those are set only on CE/Circle office rows), so the division cell is the meaningful one to compose.
        var a2 = await NewWorkAsync(_w.DivisionA, _w.HierarchyA.DivisionId);
        await AddWorkLineAsync(_w.DivisionA, _w.WorksProposalAId, _w.WorkAId, 1, 100, 10);
        await AddWorkLineAsync(_w.DivisionA, _w.WorksProposalAId, a2, 2, 250, 25);
        await AddWorkLineAsync(_w.DivisionB, _worksProposalB, _w.WorkBId, 1, 400, 40);

        var finance = await FinanceActorAsync();
        await using var ctx = _w.NewContext();

        var consolidation = await new GetConsolidationHandler(ctx, finance, TestServices.OfficeScope(ctx))
            .Handle(new GetConsolidationQuery(_w.CycleId, "WORKS", null, "division"), default);
        // Division A's WORK cell: its two lines, 100 + 250.
        var cellA = consolidation.Data!.Rows.Single(r => r.LineClass == "WORK" && r.Key == _w.HierarchyA.DivisionId.ToString());
        cellA.DemandedRbe.Should().Be(350);

        var composition = await new GetConsolidationCompositionHandler(ctx, finance, TestServices.OfficeScope(ctx))
            .Handle(new GetConsolidationCompositionQuery(_w.CycleId, $"division:{_w.HierarchyA.DivisionId}", "WORKS"), default);

        composition.IsSucced.Should().BeTrue();
        composition.Data!.Should().HaveCount(2, "Division A's two WORK lines compose its cell");
        composition.Data.Sum(l => l.DemandedRbe).Should().Be(cellA.DemandedRbe,
            "the composition lines sum exactly to the consolidated cell (BUD-178)");
        composition.Data.Sum(l => l.DemandedObeNext).Should().Be(cellA.DemandedObeNext);
    }
}
