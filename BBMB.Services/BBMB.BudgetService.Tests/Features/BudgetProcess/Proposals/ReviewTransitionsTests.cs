using System.Text.Json;
using BBMB.BudgetService.Features.BudgetProcess.Observations.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Forward;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.ForwardToFinance;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Open;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Recall;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Reopen;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Return;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Return.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.SendBack;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Submit;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Entities;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
namespace BBMB.BudgetService.Tests.Features.BudgetProcess.Proposals;
/// <summary>Design §8.3 open … reopen rows; §6.2 T05–T21; DV-07.</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class ReviewTransitionsTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;
    public async Task InitializeAsync() => _w = await SpineWorld.CreateAsync(fixture, $"rvt_{Guid.NewGuid():N}"[..20]);
    public Task DisposeAsync() => Task.CompletedTask;

    private FakeBudgetActorSource At(long officeId, string level, string role) =>
        new(_w.ItCell.Actor! with { OfficeId = officeId, OfficeLevel = level, RoleCode = role });
    private FakeBudgetActorSource Ce => At(_w.CeOfficeId, "CE", "CE");
    private FakeBudgetActorSource Sec => At(_w.SecretariatOfficeId, "SECRETARIAT", "SECRETARY");
    private FakeBudgetActorSource Fin => At(_w.FinanceOfficeId, "FINANCE", "AFA_BUDGET");

    private async Task<Response> Run(FakeBudgetActorSource actor, Func<BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts.BudgetContext,
        BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules.ProposalTransitionService, Task<Response>> f)
    {
        await using var ctx = _w.NewContext();
        return await f(ctx, TestServices.Transitions(ctx, actor));
    }

    private Task<Response> Submit(FakeBudgetActorSource a, long id) =>
        Run(a, (c, t) => new SubmitProposalHandler(c, a, t, new DemandLineRules(new DemandLineRuleInputsLoader(c))).Handle(new SubmitProposalCommand(id), default));
    private Task<Response> Open(FakeBudgetActorSource a, long id) =>
        Run(a, (_, t) => new OpenProposalHandler(t).Handle(new OpenProposalCommand(id), default));
    private Task<Response> Forward(FakeBudgetActorSource a, long id) =>
        Run(a, (c, t) => new ForwardProposalHandler(c, a, t, new DemandLineRules(new DemandLineRuleInputsLoader(c))).Handle(new ForwardProposalCommand(id, null), default));
    private Task<Response> Return(FakeBudgetActorSource a, long id, string? remark, List<long>? lines = null) =>
        Run(a, (c, t) => new ReturnProposalHandler(c, t).Handle(new ReturnProposalCommand(id, new ReturnProposalDto { Remark = remark, SelectedDemandLineIds = lines }), default));
    private Task<Response> ToFinance(FakeBudgetActorSource a, long id) =>
        Run(a, (_, t) => new ForwardToFinanceProposalHandler(t).Handle(new ForwardToFinanceProposalCommand(id, null), default));

    private static string State(Response r)
    {
        r.IsSucced.Should().BeTrue(r.Message);
        return ((Response<ProposalStateResponseDto>)r).Data!.StateCode;
    }

    private async Task<long> NewProposalAsync(string category, long officeId)
    {
        await using var ctx = _w.NewContext();
        await using var tx = await new BudgetActionScope(ctx, _w.ItCell).BeginAsync("OPEN_PROPOSALS");
        var p = new BudgetProposal { CycleId = _w.CycleId, Category = category, OriginOfficeId = officeId, HolderOfficeId = officeId, StateCode = "S-01" };
        ctx.BudgetProposals.Add(p);
        await ctx.SaveChangesAsync();
        await tx.CommitAsync();
        return p.ProposalId;
    }

    private async Task<long> LineAsync(int workId)
    {
        await using var ctx = _w.NewContext();
        await using var tx = await new BudgetActionScope(ctx, _w.DivisionA).BeginAsync("DEMAND_LINE_ADD");
        var serial = (await ctx.DemandLines.Where(l => l.ProposalId == _w.WorksProposalAId).MaxAsync(l => (int?)l.SerialNo) ?? 0) + 1;
        var l = new DemandLine { ProposalId = _w.WorksProposalAId, LineType = "WORK", WorkId = workId, AccountCodeId = _w.AccountCodeId, SerialNo = serial, RbeAmount = 1, ObeNextAmount = 1 };
        ctx.DemandLines.Add(l);
        await ctx.SaveChangesAsync();
        await tx.CommitAsync();
        return l.DemandLineId;
    }

    private async Task ObserveAsync(FakeBudgetActorSource reviewer, long? lineId, string text)
    {
        await using var ctx = _w.NewContext();
        await using var tx = await new BudgetActionScope(ctx, reviewer).BeginAsync("OBSERVATION_RECORD");
        ctx.Observations.Add(new Observation { ProposalId = _w.WorksProposalAId, DemandLineId = lineId, LevelOfficeId = reviewer.Actor!.OfficeId, ObservationText = text });
        await ctx.SaveChangesAsync();
        await tx.CommitAsync();
    }

    [Fact]
    public async Task T05_T18_full_round_trip_of_a_Division_WORKS_proposal_reaches_S10()
    {
        var id = _w.WorksProposalAId;
        State(await Submit(_w.DivisionA, id)).Should().Be("S-02");
        State(await Open(_w.Circle, id)).Should().Be("S-03");
        State(await Forward(_w.Circle, id)).Should().Be("S-05");
        State(await Open(Ce, id)).Should().Be("S-06");
        State(await Forward(Ce, id)).Should().Be("S-07");
        State(await Open(Sec, id)).Should().Be("S-08");
        State(await ToFinance(Sec, id)).Should().Be("S-09");
        State(await Open(Fin, id)).Should().Be("S-10");
        State(await Run(Fin, (_, t) => new SendBackProposalHandler(t).Handle(new SendBackProposalCommand(id, "Recheck"), default))).Should().NotBe("S-10");
    }

    [Fact]
    public async Task DV07_ReturnTwoLevelsDownRefused()
    {
        var id = _w.WorksProposalAId;
        State(await Submit(_w.DivisionA, id)).Should().Be("S-02");
        State(await Open(_w.Circle, id)).Should().Be("S-03");
        State(await Forward(_w.Circle, id)).Should().Be("S-05");
        State(await Open(Ce, id)).Should().Be("S-06");
        var back = await Return(Ce, id, "Circle to examine again");
        State(back).Should().Be("S-03", "a Division-framed proposal returns one level down, to the Circle (T11)");
        ((Response<ProposalStateResponseDto>)back).Data!.HolderOfficeId.Should().Be(_w.CircleOfficeId);
        // A forged S-06 RETURN S-04 movement for the Division-framed proposal is refused by DB-12.
        State(await Forward(_w.Circle, id)).Should().Be("S-05");
        State(await Open(Ce, id)).Should().Be("S-06");
        var (c, tx) = await _w.BeginAsync(Ce, "RETURN");
        await using var _ = c;
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO budget.proposal_movement (proposal_id, from_state, action_code, to_state, from_office_id, to_office_id, remark, role_code, client_ip) " +
            $"VALUES ({id}, 'S-06', 'RETURN', 'S-04', {_w.CeOfficeId}, {_w.DivisionAOfficeId}, 'forged', 'CE', '127.0.0.1')", c, tx);
        var ex = (await FluentActions.Awaiting(() => cmd.ExecuteNonQueryAsync()).Should().ThrowAsync<PostgresException>()).Which;
        ex.MessageText.Should().Be("DB-12");
    }

    [Fact]
    public async Task T11b_a_Circle_framed_proposal_returned_by_the_CE_lands_S04()
    {
        var dna = await NewProposalAsync("DNA", _w.CircleOfficeId);
        State(await Submit(_w.Circle, dna)).Should().Be("S-05");
        State(await Open(Ce, dna)).Should().Be("S-06");
        State(await Return(Ce, dna, "Correct the heads")).Should().Be("S-04");
    }

    [Fact]
    public async Task BUD120_RETURN_without_remark_is_M12()
    {
        State(await Submit(_w.DivisionA, _w.WorksProposalAId)).Should().Be("S-02");
        State(await Open(_w.Circle, _w.WorksProposalAId)).Should().Be("S-03");
        (await Return(_w.Circle, _w.WorksProposalAId, "  ")).MessageId.Should().Be("M-12");
    }

    [Fact]
    public async Task BUD215_OPEN_withdraws_the_lower_offices_recall()
    {
        State(await Submit(_w.DivisionA, _w.WorksProposalAId)).Should().Be("S-02");
        State(await Open(_w.Circle, _w.WorksProposalAId)).Should().Be("S-03");
        (await Run(_w.DivisionA, (_, t) => new RecallProposalHandler(t).Handle(new RecallProposalCommand(_w.WorksProposalAId), default)))
            .MessageId.Should().Be("M-19");
    }

    [Fact]
    public void BUD118_ForwardToFinance_requires_Review_ForwardFinance_which_only_SECRETARY_holds()
    {
        var route = EndpointCatalog.Find("POST", "/api/budget-proposals/{id:long}/forward-to-finance");
        route.Should().NotBeNull();
        route!.Policies.Should().Equal("BudgetRight:Review.ForwardFinance");
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(path),
            new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
        doc.RootElement.GetProperty("BudgetRights").GetProperty("Rights").GetProperty("Review.ForwardFinance")
            .EnumerateArray().Select(r => r.GetString()).Should().Equal("SECRETARY");
    }

    [Fact]
    public async Task BUD118_ForwardToFinance_by_a_CE_without_the_right_is_M20()
    {
        State(await Submit(_w.DivisionA, _w.WorksProposalAId)).Should().Be("S-02");
        State(await Open(_w.Circle, _w.WorksProposalAId)).Should().Be("S-03");
        State(await Forward(_w.Circle, _w.WorksProposalAId)).Should().Be("S-05");
        State(await Open(Ce, _w.WorksProposalAId)).Should().Be("S-06");
        State(await Forward(Ce, _w.WorksProposalAId)).Should().Be("S-07");
        State(await Open(Sec, _w.WorksProposalAId)).Should().Be("S-08");
        // A Secretariat officer whose role lacks Review.ForwardFinance (the right the CE role does not hold).
        await using var ctx = _w.NewContext();
        var noRight = TestServices.Transitions(ctx, Sec, TestServices.Allow("Budget.View", "Review.Examine"));
        (await new ForwardToFinanceProposalHandler(noRight).Handle(new ForwardToFinanceProposalCommand(_w.WorksProposalAId, null), default))
            .MessageId.Should().Be("M-20");
    }

    [Fact]
    public async Task BUD106_return_stamps_the_selected_lines_unreturned_observations_and_foot_remarks()
    {
        var l1 = await LineAsync(_w.WorkAId);
        var w2 = await NewWorkAsync();
        var l2 = await LineAsync(w2);
        State(await Submit(_w.DivisionA, _w.WorksProposalAId)).Should().Be("S-02");
        State(await Open(_w.Circle, _w.WorksProposalAId)).Should().Be("S-03");
        await ObserveAsync(_w.Circle, l1, "Justify the increase");
        await ObserveAsync(_w.Circle, l2, "Recheck the estimate");
        await ObserveAsync(_w.Circle, null, "General remark");
        (await Return(_w.Circle, _w.WorksProposalAId, "Correct", [987654])).MessageId.Should().Be("M-22", "a line not of the proposal");
        State(await Return(_w.Circle, _w.WorksProposalAId, "Correct line 1", [l1])).Should().Be("S-04");
        await using var ctx = _w.NewContext();
        var movement = await ctx.ProposalMovements.AsNoTracking().Where(m => m.ProposalId == _w.WorksProposalAId && m.ActionCode == "RETURN").SingleAsync();
        var obs = await ctx.Observations.AsNoTracking().Where(o => o.ProposalId == _w.WorksProposalAId).ToListAsync();
        obs.Single(o => o.DemandLineId == l1).ReturnedByMovementId.Should().Be(movement.MovementId);
        obs.Single(o => o.DemandLineId == l1).SelectedForReturn.Should().BeTrue();
        obs.Single(o => o.DemandLineId == null).ReturnedByMovementId.Should().Be(movement.MovementId);
        obs.Single(o => o.DemandLineId == l2).ReturnedByMovementId.Should().BeNull("not selected");
    }

    private async Task<int> NewWorkAsync()
    {
        await using var ctx = _w.NewContext();
        await using var tx = await new BudgetActionScope(ctx, _w.DivisionA).BeginAsync("WORKS_ENTER");
        var s = _w.Segments;
        var w = new BBMB.BudgetService.Features.Works.Entities.Work
        {
            WorkCode = 600001, DivisionId = _w.HierarchyA.DivisionId, WorkDesc = "Second work", UserCode = "T",
            MajorAccCode = s.Major, SubMajorAccCode = s.SubMajor, MinorAccCode = s.Minor, SubMinorAccCode = s.SubMinor,
            DetailAccCode = s.Detail, ObjectAccCode = s.Object, WorkClass = "AA", EnteredVia = "LEGACY_OPENING",
        };
        ctx.Works.Add(w);
        await ctx.SaveChangesAsync();
        await tx.CommitAsync();
        return w.Id;
    }

    [Fact]
    public async Task T21_reopen_outside_S11_is_M19()
    {
        // S-11 is reached by Finalise (P1-33, where the S-11 REOPEN round trip is tested).
        (await Run(Fin, (_, t) => new ReopenProposalHandler(t).Handle(new ReopenProposalCommand(_w.WorksProposalAId, "x"), default)))
            .MessageId.Should().Be("M-19");
    }
}
