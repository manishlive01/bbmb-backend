using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.FinaliseCategory;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.FinaliseCategory.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.GetMovements;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Reopen;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Tests.Features.BudgetProcess.Proposals;
/// <summary>Design §6.2 T20 + expected set, T21; §8.3 finalise / movements rows (BUD-134, BUD-238).</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class FinaliseTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;
    public async Task InitializeAsync() => _w = await SpineWorld.CreateAsync(fixture, $"fin_{Guid.NewGuid():N}"[..20]);
    public Task DisposeAsync() => Task.CompletedTask;
    private FakeBudgetActorSource At(long officeId, string level, string role) =>
        new(_w.ItCell.Actor! with { OfficeId = officeId, OfficeLevel = level, RoleCode = role });
    private FakeBudgetActorSource Fin => At(_w.FinanceOfficeId, "FINANCE", "AFA_BUDGET");

    private async Task MoveAsync(FakeBudgetActorSource actor, long id, string action, string? remark = null)
    {
        await using var ctx = _w.NewContext();
        var r = await TestServices.Transitions(ctx, actor).MoveAsync(id, action, remark);
        r.IsSucced.Should().BeTrue($"{action}: {r.Message}");
    }

    private async Task MoveToAsync(long id, FakeBudgetActorSource division, string upTo)
    {
        var ce = At(_w.CeOfficeId, "CE", "CE");
        var sec = At(_w.SecretariatOfficeId, "SECRETARIAT", "SECRETARY");
        var steps = new (FakeBudgetActorSource, string, string)[]
        {
            (division, "SUBMIT", "S-02"), (_w.Circle, "OPEN", "S-03"), (_w.Circle, "FORWARD", "S-05"), (ce, "OPEN", "S-06"),
            (ce, "FORWARD", "S-07"), (sec, "OPEN", "S-08"), (sec, "FORWARD_FINANCE", "S-09"), (Fin, "OPEN", "S-10"),
        };
        foreach (var (actor, action, reached) in steps)
        {
            await MoveAsync(actor, id, action);
            if (reached == upTo) return;
        }
    }

    private async Task<long> ProposalBAsync()
    {
        await using var ctx = _w.NewContext();
        await using var tx = await new BudgetActionScope(ctx, _w.ItCell).BeginAsync("OPEN_PROPOSALS");
        var p = new BudgetProposal { CycleId = _w.CycleId, Category = "WORKS", OriginOfficeId = _w.DivisionBOfficeId, HolderOfficeId = _w.DivisionBOfficeId, StateCode = "S-01" };
        ctx.BudgetProposals.Add(p);
        await ctx.SaveChangesAsync();
        await tx.CommitAsync();
        return p.ProposalId;
    }

    private async Task<Response> FinaliseAsync(string category = "WORKS")
    {
        await using var ctx = _w.NewContext();
        return await new FinaliseCategoryHandler(ctx, Fin, TestServices.AllowAll(), new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
                TestServices.Transitions(ctx, Fin), new DemandLineRules(new DemandLineRuleInputsLoader(ctx)))
            .Handle(new FinaliseCategoryCommand(_w.CycleId, category), default);
    }

    private async Task<string> StateOfAsync(long id)
    {
        await using var ctx = _w.NewContext();
        return (await ctx.BudgetProposals.AsNoTracking().SingleAsync(p => p.ProposalId == id)).StateCode;
    }

    [Fact]
    public async Task BUD134_FinaliseRefusedListsDivisionWithoutProposal()
    {
        await MoveToAsync(_w.WorksProposalAId, _w.DivisionA, "S-10");
        var r = await FinaliseAsync();
        r.MessageId.Should().Be("M-19");
        ((Response<FinaliseResponseDto>)r).Data!.MissingOffices.Should().Equal(_w.DivisionBOfficeId);
        (await StateOfAsync(_w.WorksProposalAId)).Should().Be("S-10", "nothing moves");
    }

    [Fact]
    public async Task BUD134_one_proposal_at_S08_is_listed_as_outstanding_and_nothing_moves()
    {
        var b = await ProposalBAsync();
        await MoveToAsync(_w.WorksProposalAId, _w.DivisionA, "S-10");
        await MoveToAsync(b, _w.DivisionB, "S-08");
        var r = await FinaliseAsync();
        r.MessageId.Should().Be("M-19");
        ((Response<FinaliseResponseDto>)r).Data!.Outstanding.Should().ContainSingle()
            .Which.Should().Be(new OutstandingProposalDto(b, _w.DivisionBOfficeId, "S-08"));
        (await StateOfAsync(_w.WorksProposalAId)).Should().Be("S-10");
    }

    [Fact]
    public async Task T20_all_at_S10_moves_every_proposal_to_S11_with_one_movement_each_and_T21_reopens_with_a_remark()
    {
        var b = await ProposalBAsync();
        await MoveToAsync(_w.WorksProposalAId, _w.DivisionA, "S-10");
        await MoveToAsync(b, _w.DivisionB, "S-10");
        var r = await FinaliseAsync();
        r.IsSucced.Should().BeTrue(r.Message);
        ((Response<FinaliseResponseDto>)r).Data!.Finalised.Should().BeEquivalentTo(new[] { _w.WorksProposalAId, b });
        await using (var ctx = _w.NewContext())
        {
            (await ctx.BudgetProposals.AsNoTracking().Where(p => p.CycleId == _w.CycleId && p.Category == "WORKS").Select(p => p.StateCode).ToListAsync())
                .Should().OnlyContain(s => s == "S-11");
            (await ctx.ProposalMovements.AsNoTracking().Where(m => m.ActionCode == "FINALISE").GroupBy(m => m.ProposalId).Select(g => g.Count()).ToListAsync())
                .Should().Equal(1, 1);
        }
        (await FinaliseAsync()).MessageId.Should().Be("M-19", "nothing left to finalise");
        await using (var ctx = _w.NewContext())
        {
            var reopen = new ReopenProposalHandler(TestServices.Transitions(ctx, Fin));
            (await reopen.Handle(new ReopenProposalCommand(b, null), default)).MessageId.Should().Be("M-12");
            var back = await reopen.Handle(new ReopenProposalCommand(b, "Correction found"), default);
            ((Response<ProposalStateResponseDto>)back).Data!.StateCode.Should().Be("S-10");
        }
    }

    [Fact]
    public async Task BUD238_movements_are_listed_in_order_with_offices_and_officers()
    {
        await MoveToAsync(_w.WorksProposalAId, _w.DivisionA, "S-05");
        await using var ctx = _w.NewContext();
        var r = await new GetMovementsHandler(ctx, _w.DivisionA, TestServices.OfficeScope(ctx)).Handle(new GetMovementsQuery(_w.WorksProposalAId), default);
        r.Data!.Select(m => (m.ActionCode, m.FromOfficeId, m.ToOfficeId)).Should().Equal(
            ("SUBMIT", _w.DivisionAOfficeId, _w.CircleOfficeId),
            ("OPEN", _w.CircleOfficeId, _w.CircleOfficeId),
            ("FORWARD", _w.CircleOfficeId, _w.CeOfficeId));
        r.Data.Should().OnlyContain(m => m.OfficerId == _w.ItCell.Actor!.EmployeeId && m.FromOfficeName != null);
        (await new GetMovementsHandler(ctx, _w.DivisionB, TestServices.OfficeScope(ctx)).Handle(new GetMovementsQuery(_w.WorksProposalAId), default))
            .MessageId.Should().Be("NOT_FOUND");
    }
}
