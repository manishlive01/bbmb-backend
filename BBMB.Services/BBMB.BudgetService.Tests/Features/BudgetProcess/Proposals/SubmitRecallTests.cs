using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Add;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Add.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Recall;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Submit;
using BBMB.BudgetService.Features.Works.Rules;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
namespace BBMB.BudgetService.Tests.Features.BudgetProcess.Proposals;
/// <summary>Design §8.3 submit / recall rows; §6.2 T01–T04b, T08–T09c, T13–T13c, T17.</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class SubmitRecallTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;
    public async Task InitializeAsync() => _w = await SpineWorld.CreateAsync(fixture, $"sur_{Guid.NewGuid():N}"[..20]);
    public Task DisposeAsync() => Task.CompletedTask;

    private FakeBudgetActorSource Ce => new(_w.ItCell.Actor! with { OfficeId = _w.CeOfficeId, OfficeLevel = "CE", RoleCode = "CE" });
    private FakeBudgetActorSource Secretariat =>
        new(_w.ItCell.Actor! with { OfficeId = _w.SecretariatOfficeId, OfficeLevel = "SECRETARIAT", RoleCode = "SECRETARY" });

    private async Task<Response> SubmitAsync(FakeBudgetActorSource actor, long proposalId)
    {
        await using var ctx = _w.NewContext();
        return await new SubmitProposalHandler(ctx, actor, TestServices.Transitions(ctx, actor), new DemandLineRules(new DemandLineRuleInputsLoader(ctx)))
            .Handle(new SubmitProposalCommand(proposalId), default);
    }

    private async Task<Response> RecallAsync(FakeBudgetActorSource actor, long proposalId)
    {
        await using var ctx = _w.NewContext();
        return await new RecallProposalHandler(TestServices.Transitions(ctx, actor)).Handle(new RecallProposalCommand(proposalId), default);
    }

    private async Task MoveAsync(FakeBudgetActorSource actor, long proposalId, string action, string? remark = null)
    {
        await using var ctx = _w.NewContext();
        var r = await TestServices.Transitions(ctx, actor).MoveAsync(proposalId, action, remark);
        r.IsSucced.Should().BeTrue($"{action}: {r.Message}");
    }

    private static string StateOf(Response r)
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

    private async Task<long> AddWorkLineAsync(decimal? rbe, decimal? obe)
    {
        await using var ctx = _w.NewContext();
        var r = await new AddDemandLineHandler(ctx, _w.DivisionA, new BudgetActionScope(ctx, _w.DivisionA), TestServices.OfficeScope(ctx),
                TestServices.AllowAll(), new HttpContextAccessor { HttpContext = new DefaultHttpContext() }, new ConfigurationBuilder().Build(),
                new DemandLineRules(new DemandLineRuleInputsLoader(ctx)), new WorkRules(ctx))
            .Handle(new AddDemandLineCommand(_w.WorksProposalAId, new AddDemandLineDto { LineType = "WORK", WorkId = _w.WorkAId, RbeAmount = rbe, ObeNextAmount = obe }), default);
        r.IsSucced.Should().BeTrue(r.Message);
        return ((Response<BBMB.BudgetService.Features.BudgetProcess.DemandLines.Dtos.SaveDemandLineResponseDto>)r).Data!.DemandLineId;
    }

    [Fact]
    public async Task BUD096_SubmitWithOneFailingLineMovesNothing()
    {
        var line = await AddWorkLineAsync(rbe: null, obe: 10); // saved as a part entry (BUD-241)
        var r = await SubmitAsync(_w.DivisionA, _w.WorksProposalAId);
        r.IsSucced.Should().BeFalse();
        r.MessageId.Should().Be("M-21");
        ((Response<RuleFailuresDto>)r).Data!.Failures.Should().ContainSingle()
            .Which.Should().Match<RuleFailureItemDto>(f => f.DemandLineId == line && f.SerialNo == 1 && f.Field == "rbeAmount" && f.MessageId == "M-21");
        await using var ctx = _w.NewContext();
        var p = await ctx.BudgetProposals.AsNoTracking().SingleAsync(x => x.ProposalId == _w.WorksProposalAId);
        p.StateCode.Should().Be("S-01");
        p.FirstSubmittedAt.Should().BeNull();
        (await ctx.ProposalMovements.AsNoTracking().AnyAsync(m => m.ProposalId == _w.WorksProposalAId)).Should().BeFalse("no T-33 row");
    }

    [Fact]
    public async Task S01_complete_submit_moves_to_S02_and_sets_first_submitted_at_then_RECALL_returns_to_S01()
    {
        await AddWorkLineAsync(rbe: 10, obe: 20);
        StateOf(await SubmitAsync(_w.DivisionA, _w.WorksProposalAId)).Should().Be("S-02");
        StateOf(await RecallAsync(_w.DivisionA, _w.WorksProposalAId)).Should().Be("S-01");
        await using var ctx = _w.NewContext();
        (await ctx.BudgetProposals.AsNoTracking().SingleAsync(x => x.ProposalId == _w.WorksProposalAId)).FirstSubmittedAt.Should().NotBeNull();
        (await ctx.AuditLog.AsNoTracking().AnyAsync(a => a.TableName == "proposal_movement" && a.ActionCode == "RECALL")).Should().BeTrue("BUD-217");
    }

    [Fact]
    public async Task BUD214_RecallAfterResubmitLandsInS04_Division_T04b()
    {
        StateOf(await SubmitAsync(_w.DivisionA, _w.WorksProposalAId)).Should().Be("S-02");
        await MoveAsync(_w.Circle, _w.WorksProposalAId, "OPEN");
        await MoveAsync(_w.Circle, _w.WorksProposalAId, "RETURN", "Please correct");
        StateOf(await SubmitAsync(_w.DivisionA, _w.WorksProposalAId)).Should().Be("S-02", "RESUBMIT from S-04 (T08)");
        StateOf(await RecallAsync(_w.DivisionA, _w.WorksProposalAId)).Should().Be("S-04");
    }

    [Fact]
    public async Task BUD214_RecallAfterResubmitLandsInS04_CircleOrigin_T09c()
    {
        var dna = await NewProposalAsync("DNA", _w.CircleOfficeId);
        StateOf(await SubmitAsync(_w.Circle, dna)).Should().Be("S-05", "a Circle-framed proposal submits to the CE (T02)");
        await MoveAsync(Ce, dna, "OPEN");
        await MoveAsync(Ce, dna, "RETURN", "Please correct");
        StateOf(await SubmitAsync(_w.Circle, dna)).Should().Be("S-05", "T08b");
        StateOf(await RecallAsync(_w.Circle, dna)).Should().Be("S-04");
    }

    [Fact]
    public async Task BUD214_RecallAfterResubmitLandsInS04_CeOrigin_T13c()
    {
        var dna = await NewProposalAsync("DNA", _w.CeOfficeId);
        StateOf(await SubmitAsync(Ce, dna)).Should().Be("S-07", "a CE-framed proposal submits to the Secretariat (T03)");
        await MoveAsync(Secretariat, dna, "OPEN");
        await MoveAsync(Secretariat, dna, "RETURN", "Please correct");
        StateOf(await SubmitAsync(Ce, dna)).Should().Be("S-07", "T08c");
        StateOf(await RecallAsync(Ce, dna)).Should().Be("S-04");
    }

    [Fact]
    public async Task BUD215_recall_after_OPEN_and_recall_by_another_office_are_M19()
    {
        StateOf(await SubmitAsync(_w.DivisionA, _w.WorksProposalAId)).Should().Be("S-02");
        (await RecallAsync(_w.Circle, _w.WorksProposalAId)).MessageId.Should().Be("M-19", "only the office that moved it may recall");
        await MoveAsync(_w.Circle, _w.WorksProposalAId, "OPEN");
        (await RecallAsync(_w.DivisionA, _w.WorksProposalAId)).MessageId.Should().Be("M-19", "opened above: no recall");
    }

    [Fact]
    public async Task S62_an_empty_head_wise_proposal_submits_as_a_nil_demand()
    {
        var dna = await NewProposalAsync("DNA", _w.DivisionAOfficeId);
        StateOf(await SubmitAsync(_w.DivisionA, dna)).Should().Be("S-02");
    }

    [Fact]
    public async Task S82_submit_without_F07_Submit_is_M20()
    {
        await using var ctx = _w.NewContext();
        var r = await new SubmitProposalHandler(ctx, _w.DivisionA,
                TestServices.Transitions(ctx, _w.DivisionA, TestServices.Allow("Budget.View", "F07.Enter")),
                new DemandLineRules(new DemandLineRuleInputsLoader(ctx)))
            .Handle(new SubmitProposalCommand(_w.WorksProposalAId), default);
        r.MessageId.Should().Be("M-20");
    }
}
