using BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.Assign;
using BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.Assign.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.GetAssignableHeads;
using BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.GetList;
using BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.Remove;
using BBMB.BudgetService.Features.BudgetProcess.Cycles.OpenProposals;
using BBMB.BudgetService.Features.BudgetProcess.Cycles.OpenProposals.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Entities;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
namespace BBMB.BudgetService.Tests.Features.BudgetProcess.AccountHeadAssignments;
/// <summary>Design §8.4 assignment rows, §6.2 expected set (F-17; VR-06, BUD-168 to BUD-171; DB-01, DB-02).</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class AssignmentTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;
    public async Task InitializeAsync() => _w = await SpineWorld.CreateAsync(fixture, $"ahs_{Guid.NewGuid():N}"[..20]);
    public Task DisposeAsync() => Task.CompletedTask;

    private FakeBudgetActorSource Afa => new(_w.ItCell.Actor! with { OfficeId = _w.FinanceOfficeId, OfficeLevel = "FINANCE", RoleCode = "AFA_BUDGET" });

    private async Task<Response<int>> AssignAsync(long officeId, params long[] codes)
    {
        await using var ctx = _w.NewContext();
        return await new AssignHeadsHandler(ctx, Afa, new BudgetActionScope(ctx, Afa))
            .Handle(new AssignHeadsCommand(new AssignHeadsDto { OfficeId = officeId, AccountCodeIds = [.. codes] }), default);
    }

    private async Task<Response> RemoveAsync(long assignmentId)
    {
        await using var ctx = _w.NewContext();
        return await new RemoveHeadAssignmentHandler(ctx, Afa, new BudgetActionScope(ctx, Afa))
            .Handle(new RemoveHeadAssignmentCommand(assignmentId), default);
    }

    private async Task<long> AssignmentIdAsync(long officeId, long code)
    {
        await using var ctx = _w.NewContext();
        return await ctx.AccountHeadAssignments.Where(a => a.OfficeId == officeId && a.AccountCodeId == code && a.RemovedAt == null)
            .Select(a => a.AssignmentId).SingleAsync();
    }

    private async Task<long> NewCodeAsync(bool notForUse = false)
    {
        var seg = await TestMasters.InsertSegmentsAsync(_w.ConnectionString);
        var (c, tx) = await _w.BeginAsync(_w.ItCell, "TEST_SETUP");
        await using var _ = c;
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO budget.budget_head_master (major_acc_code, sub_major_acc_code, minor_acc_code, sub_minor_acc_code, " +
            "detail_acc_code, object_acc_code, head_desc, party_emp_required, origin, modification_date, user_code" +
            (notForUse ? ", not_for_use, not_for_use_ref, not_for_use_at, not_for_use_by" : "") + ") " +
            $"VALUES ('{seg.Major}', '{seg.SubMajor}', '{seg.Minor}', '{seg.SubMinor}', '{seg.Detail}', '{seg.Object}', 'Head', 'N', 'LEGACY', now(), 'T'" +
            (notForUse ? $", true, 'EO/1', now(), '{_w.ItCell.Actor!.EmployeeId}'" : "") + ") RETURNING account_code_id", c, tx);
        var id = Convert.ToInt64(await cmd.ExecuteScalarAsync());
        await tx.CommitAsync();
        return id;
    }

    private async Task<long> DnaProposalWithLineAsync(long officeId, FakeBudgetActorSource actor, long code)
    {
        await using var ctx = _w.NewContext();
        long proposalId;
        await using (var tx = await new BudgetActionScope(ctx, _w.ItCell).BeginAsync("OPEN_PROPOSALS"))
        {
            var p = new BudgetProposal { CycleId = _w.CycleId, Category = "DNA", OriginOfficeId = officeId, HolderOfficeId = officeId, StateCode = "S-01" };
            ctx.BudgetProposals.Add(p);
            await ctx.SaveChangesAsync();
            await tx.CommitAsync();
            proposalId = p.ProposalId;
        }
        await using var lctx = _w.NewContext();
        await using var ltx = await new BudgetActionScope(lctx, actor).BeginAsync("DEMAND_LINE_ADD");
        lctx.DemandLines.Add(new DemandLine { ProposalId = proposalId, LineType = "HEAD", AccountCodeId = code, SerialNo = 1 });
        await lctx.SaveChangesAsync();
        await ltx.CommitAsync();
        return proposalId;
    }

    [Fact]
    public async Task DB01_assignment_to_a_FINANCE_office_is_refused()
    {
        var code = await NewCodeAsync();
        (await AssignAsync(_w.FinanceOfficeId, code)).MessageId.Should().Be(BudgetMessages.M22);
        await using var ctx = _w.NewContext();
        (await ctx.AccountHeadAssignments.AnyAsync(a => a.AccountCodeId == code)).Should().BeFalse();
    }

    [Fact]
    public async Task BUD171_new_heads_are_assigned_with_source_AFA_and_already_assigned_are_skipped_and_reported()
    {
        var code = await NewCodeAsync();
        var r = await AssignAsync(_w.DivisionAOfficeId, code, _w.AccountCodeId);
        r.IsSucced.Should().BeTrue(r.Message);
        r.Data.Should().Be(1);
        r.Message.Should().Contain("skipped").And.Contain(_w.AccountCodeId.ToString());
        await using var ctx = _w.NewContext();
        (await ctx.AccountHeadAssignments.SingleAsync(a => a.AccountCodeId == code)).Source.Should().Be("AFA");
    }

    [Fact]
    public async Task BUD308_a_code_marked_not_for_use_is_M05_at_F17_Assign_and_nothing_is_assigned()
    {
        var good = await NewCodeAsync();
        var nfu = await NewCodeAsync(notForUse: true);
        (await AssignAsync(_w.DivisionAOfficeId, good, nfu)).MessageId.Should().Be(BudgetMessages.M05);
        await using var ctx = _w.NewContext();
        (await ctx.AccountHeadAssignments.AnyAsync(a => a.AccountCodeId == good)).Should().BeFalse("all or nothing");
    }

    [Fact]
    public async Task BUD170_remove_with_a_live_line_is_M15_and_without_one_succeeds_once()
    {
        await DnaProposalWithLineAsync(_w.DivisionAOfficeId, _w.DivisionA, _w.AccountCodeId);
        (await RemoveAsync(await AssignmentIdAsync(_w.DivisionAOfficeId, _w.AccountCodeId))).MessageId.Should().Be(BudgetMessages.M15);

        var b = await AssignmentIdAsync(_w.DivisionBOfficeId, _w.AccountCodeId);
        (await RemoveAsync(b)).IsSucced.Should().BeTrue();
        (await RemoveAsync(b)).MessageId.Should().Be(BudgetMessages.M19);
        await using var ctx = _w.NewContext();
        var row = await ctx.AccountHeadAssignments.AsNoTracking().SingleAsync(a => a.AssignmentId == b);
        row.RemovedBy.Should().Be(Afa.Actor!.EmployeeId);
    }

    [Fact]
    public async Task VR06_assignable_heads_exclude_removed_and_not_for_use_heads()
    {
        var second = await NewCodeAsync();
        (await AssignAsync(_w.CircleOfficeId, second)).IsSucced.Should().BeTrue();
        var proposal = await DnaProposalWithLineAsync(_w.CircleOfficeId, _w.Circle, _w.AccountCodeId);
        var removable = await NewCodeAsync();
        (await AssignAsync(_w.CircleOfficeId, removable)).IsSucced.Should().BeTrue();
        (await RemoveAsync(await AssignmentIdAsync(_w.CircleOfficeId, removable))).IsSucced.Should().BeTrue();
        var (c, tx) = await _w.BeginAsync(_w.ItCell, "F01_MARK_NFU");
        await using (c)
        {
            await using var cmd = new NpgsqlCommand(
                "UPDATE budget.budget_head_master SET not_for_use = true, not_for_use_ref = 'EO/3', not_for_use_at = now(), " +
                $"not_for_use_by = '{_w.ItCell.Actor!.EmployeeId}' WHERE account_code_id = {second}", c, tx);
            await cmd.ExecuteNonQueryAsync();
            await tx.CommitAsync();
        }
        await using var ctx = _w.NewContext();
        var heads = await new GetAssignableHeadsHandler(ctx, _w.Circle, TestServices.OfficeScope(ctx))
            .Handle(new GetAssignableHeadsQuery(proposal), default);
        heads.Data!.Select(h => h.AccountCodeId).Should().Equal(_w.AccountCodeId);
        var listed = await new GetHeadAssignmentsHandler(ctx, _w.Circle, TestServices.OfficeScope(ctx))
            .Handle(new GetHeadAssignmentsQuery(null), default);
        listed.Data!.Select(h => (h.AccountCodeId, h.NotForUse)).Should().BeEquivalentTo([(_w.AccountCodeId, false), (second, true)]);
    }

    [Fact]
    public async Task BUD031_a_DDO_sees_only_its_own_heads()
    {
        await using var ctx = _w.NewContext();
        (await new GetHeadAssignmentsHandler(ctx, _w.DivisionB, TestServices.OfficeScope(ctx))
            .Handle(new GetHeadAssignmentsQuery(_w.DivisionAOfficeId), default)).MessageId.Should().Be(BudgetMessages.NotFound);
        (await new GetHeadAssignmentsHandler(ctx, _w.DivisionB, TestServices.OfficeScope(ctx))
            .Handle(new GetHeadAssignmentsQuery(null), default)).Data!.Should().ContainSingle();
    }

    [Fact]
    public async Task S62_open_proposals_creates_head_wise_proposals_for_offices_with_live_assignments()
    {
        (await RemoveAsync(await AssignmentIdAsync(_w.DivisionBOfficeId, _w.AccountCodeId))).IsSucced.Should().BeTrue();
        await using var ctx = _w.NewContext();
        var r = await new OpenProposalsHandler(ctx, _w.ItCell, new BudgetActionScope(ctx, _w.ItCell))
            .Handle(new OpenProposalsCommand(_w.CycleId, new OpenProposalsDto { Categories = ["DNA"] }), default);
        r.IsSucced.Should().BeTrue(r.Message);
        r.Data!.Created.Select(p => p.OfficeId).Should().BeEquivalentTo(
            new[] { _w.CeOfficeId, _w.CircleOfficeId, _w.DivisionAOfficeId }, "Division B's only assignment was removed");
    }

    [Fact]
    public void Routes_carry_F17_Assign_and_Budget_View()
    {
        EndpointCatalog.Find("POST", "/api/account-head-assignments")!.Policies.Should().Equal("BudgetRight:F17.Assign");
        EndpointCatalog.Find("POST", "/api/account-head-assignments/{id:long}/remove")!.Policies.Should().Equal("BudgetRight:F17.Assign");
        EndpointCatalog.Find("GET", "/api/account-head-assignments")!.Policies.Should().Equal("BudgetRight:Budget.View");
        EndpointCatalog.Find("GET", "/api/budget-proposals/{id:long}/assignable-heads")!.Policies.Should().Equal("BudgetRight:Budget.View");
    }
}
