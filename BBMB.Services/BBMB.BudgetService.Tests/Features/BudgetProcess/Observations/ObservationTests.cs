using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Add;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Add.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Update;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Update.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Observations.GetByProposal;
using BBMB.BudgetService.Features.BudgetProcess.Observations.Record;
using BBMB.BudgetService.Features.BudgetProcess.Observations.Record.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Observations.Reply;
using BBMB.BudgetService.Features.BudgetProcess.Observations.Reply.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Return;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Return.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Submit;
using BBMB.BudgetService.Features.Works.Rules;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
namespace BBMB.BudgetService.Tests.Features.BudgetProcess.Observations;
/// <summary>Design §5.3 T-32, §8.3 observation rows (BUD-120, BUD-121, BUD-128, DB-15).</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class ObservationTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;
    private static readonly IConfiguration Config = new ConfigurationBuilder().Build();
    public async Task InitializeAsync() => _w = await SpineWorld.CreateAsync(fixture, $"obs_{Guid.NewGuid():N}"[..20]);
    public Task DisposeAsync() => Task.CompletedTask;
    private static HttpContextAccessor Http() => new() { HttpContext = new DefaultHttpContext() };

    private async Task<long> AddLineAsync()
    {
        await using var ctx = _w.NewContext();
        var r = await new AddDemandLineHandler(ctx, _w.DivisionA, new BudgetActionScope(ctx, _w.DivisionA), TestServices.OfficeScope(ctx),
                TestServices.AllowAll(), Http(), Config, new DemandLineRules(new DemandLineRuleInputsLoader(ctx)), new WorkRules(ctx))
            .Handle(new AddDemandLineCommand(_w.WorksProposalAId, new AddDemandLineDto { LineType = "WORK", WorkId = _w.WorkAId, RbeAmount = 10, ObeNextAmount = 10 }), default);
        return ((Response<SaveDemandLineResponseDto>)r).Data!.DemandLineId;
    }

    private async Task<Response<long>> RecordAsync(FakeBudgetActorSource actor, long proposalId, long? lineId, string text)
    {
        await using var ctx = _w.NewContext();
        return await new RecordObservationHandler(ctx, actor, new BudgetActionScope(ctx, actor), TestServices.OfficeScope(ctx),
                TestServices.AllowAll(), Http(), Config)
            .Handle(new RecordObservationCommand(proposalId, new RecordObservationDto { DemandLineId = lineId, ObservationText = text }), default);
    }

    private async Task<Response> ReplyAsync(long obsId, string text)
    {
        await using var ctx = _w.NewContext();
        return await new ReplyObservationHandler(ctx, _w.DivisionA, new BudgetActionScope(ctx, _w.DivisionA), TestServices.OfficeScope(ctx), Config)
            .Handle(new ReplyObservationCommand(_w.WorksProposalAId, obsId, new ReplyObservationDto { ReplyText = text }), default);
    }

    private async Task<Response> SubmitAsync()
    {
        await using var ctx = _w.NewContext();
        return await new SubmitProposalHandler(ctx, _w.DivisionA, TestServices.Transitions(ctx, _w.DivisionA), new DemandLineRules(new DemandLineRuleInputsLoader(ctx)))
            .Handle(new SubmitProposalCommand(_w.WorksProposalAId), default);
    }

    private async Task MoveAsync(FakeBudgetActorSource actor, string action)
    {
        await using var ctx = _w.NewContext();
        (await TestServices.Transitions(ctx, actor).MoveAsync(_w.WorksProposalAId, action, null)).IsSucced.Should().BeTrue(action);
    }

    private async Task<Response> ReturnAsync()
    {
        await using var ctx = _w.NewContext();
        return await new ReturnProposalHandler(ctx, TestServices.Transitions(ctx, _w.Circle))
            .Handle(new ReturnProposalCommand(_w.WorksProposalAId, new ReturnProposalDto { Remark = "See observations" }), default);
    }

    [Fact]
    public void BUD120_blank_text_is_refused_by_the_validator_with_M12()
    {
        var v = new RecordObservationValidator();
        v.Validate(new RecordObservationCommand(1, new RecordObservationDto { ObservationText = "   " }))
            .Errors.Should().Contain(e => e.ErrorCode == "M-12");
        v.Validate(new RecordObservationCommand(1, new RecordObservationDto { ObservationText = new string('x', 4001) })).IsValid.Should().BeFalse();
        v.Validate(new RecordObservationCommand(1, new RecordObservationDto { ObservationText = "Fine" })).IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task DB04_a_line_of_another_proposal_is_refused_and_recording_needs_the_holder_at_a_review_state()
    {
        var line = await AddLineAsync();
        (await RecordAsync(_w.Circle, _w.WorksProposalAId, line, "Too early")).MessageId.Should().Be("M-20", "the Circle does not hold it at S-01");
        (await RecordAsync(_w.DivisionA, _w.WorksProposalAId, line, "Own remark")).MessageId.Should().Be("M-19", "S-01 is not a review state");
        (await SubmitAsync()).IsSucced.Should().BeTrue();
        await MoveAsync(_w.Circle, "OPEN");
        long dnaId;
        await using (var ctx = _w.NewContext())
        await using (var tx = await new BudgetActionScope(ctx, _w.ItCell).BeginAsync("OPEN_PROPOSALS"))
        {
            var dna = new BudgetProposal { CycleId = _w.CycleId, Category = "DNA", OriginOfficeId = _w.DivisionAOfficeId, HolderOfficeId = _w.DivisionAOfficeId, StateCode = "S-01" };
            ctx.BudgetProposals.Add(dna);
            await ctx.SaveChangesAsync();
            await tx.CommitAsync();
            dnaId = dna.ProposalId;
        }
        long otherLine;
        await using (var ctx = _w.NewContext())
        await using (var tx = await new BudgetActionScope(ctx, _w.DivisionA).BeginAsync("DEMAND_LINE_ADD"))
        {
            var l = new BBMB.BudgetService.Features.BudgetProcess.DemandLines.Entities.DemandLine
                { ProposalId = dnaId, LineType = "HEAD", AccountCodeId = _w.AccountCodeId, SerialNo = 1 };
            ctx.DemandLines.Add(l);
            await ctx.SaveChangesAsync();
            await tx.CommitAsync();
            otherLine = l.DemandLineId;
        }
        (await RecordAsync(_w.Circle, _w.WorksProposalAId, otherLine, "Line of the DNA proposal")).MessageId.Should().Be("NOT_FOUND");
        (await RecordAsync(_w.Circle, _w.WorksProposalAId, line, "Justify")).IsSucced.Should().BeTrue();
        (await RecordAsync(_w.Circle, _w.WorksProposalAId, null, "Foot remark")).IsSucced.Should().BeTrue("BUD-128");
    }

    [Fact]
    public async Task BUD121_reply_once_at_S04_to_a_returned_observation_and_the_list_shows_the_returning_movement()
    {
        var line = await AddLineAsync();
        (await ReplyAsync(1, "At S-01")).MessageId.Should().Be("M-19", "a reply is made at S-04 only");
        (await SubmitAsync()).IsSucced.Should().BeTrue();
        await MoveAsync(_w.Circle, "OPEN");
        var obs = (await RecordAsync(_w.Circle, _w.WorksProposalAId, line, "Justify the figure")).Data;
        (await ReplyAsync(obs, "Too early")).MessageId.Should().Be("M-19", "the Division is the origin, but S-03 is not S-04");
        (await ReturnAsync()).IsSucced.Should().BeTrue();
        (await ReplyAsync(obs, "Rates of 2026")).IsSucced.Should().BeTrue();
        (await ReplyAsync(obs, "Second reply")).MessageId.Should().Be("M-19", "DB-15: the reply is written once");
        await using var ctx = _w.NewContext();
        var list = (await new GetObservationsByProposalHandler(ctx, _w.DivisionA, TestServices.OfficeScope(ctx))
            .Handle(new GetObservationsByProposalQuery(_w.WorksProposalAId), default)).Data!;
        var movement = await ctx.ProposalMovements.AsNoTracking().SingleAsync(m => m.ProposalId == _w.WorksProposalAId && m.ActionCode == "RETURN");
        list.Should().ContainSingle().Which.Should().Match<BBMB.BudgetService.Features.BudgetProcess.Observations.GetByProposal.Dtos.ObservationResponseDto>(o =>
            o.ReturnedByMovementId == movement.MovementId && o.ReplyText == "Rates of 2026" && o.SerialNo == 1 &&
            o.LevelOfficeId == _w.CircleOfficeId && o.RepliedBy == _w.DivisionA.Actor!.EmployeeId);
    }

    [Fact]
    public async Task BUD121_resubmit_is_M21_while_a_returned_line_has_an_observation_and_no_justification()
    {
        var line = await AddLineAsync();
        (await SubmitAsync()).IsSucced.Should().BeTrue();
        await MoveAsync(_w.Circle, "OPEN");
        (await RecordAsync(_w.Circle, _w.WorksProposalAId, line, "Justify")).IsSucced.Should().BeTrue();
        (await ReturnAsync()).IsSucced.Should().BeTrue();
        var refused = await SubmitAsync();
        refused.MessageId.Should().Be("M-21");
        ((Response<RuleFailuresDto>)refused).Data!.Failures.Should().ContainSingle(f => f.Field == "justificationText" && f.DemandLineId == line);
        await using (var ctx = _w.NewContext())
            (await new UpdateDemandLineHandler(ctx, _w.DivisionA, new BudgetActionScope(ctx, _w.DivisionA), TestServices.OfficeScope(ctx),
                    TestServices.AllowAll(), Http(), Config, new DemandLineRules(new DemandLineRuleInputsLoader(ctx)))
                .Handle(new UpdateDemandLineCommand(_w.WorksProposalAId, line,
                    new UpdateDemandLineDto { RbeAmount = 10, ObeNextAmount = 10, JustificationText = "Rates revised" }), default))
                .IsSucced.Should().BeTrue();
        (await SubmitAsync()).IsSucced.Should().BeTrue("RESUBMIT from S-04 once justified");
    }
}
