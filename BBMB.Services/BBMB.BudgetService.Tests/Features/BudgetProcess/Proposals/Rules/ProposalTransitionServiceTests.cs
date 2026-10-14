using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Tests.Features.BudgetProcess.Proposals.Rules;

/// <summary>Design §4.9 (scope and holder rule), §6.2, §11.4 (T-16 absent).</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class ProposalTransitionServiceTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;

    public async Task InitializeAsync() => _w = await SpineWorld.CreateAsync(fixture, $"pts_{Guid.NewGuid():N}"[..20]);

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<BBMB.BudgetService.Shared.ResponseDTOs.Response<BBMB.BudgetService.Features.BudgetProcess.Proposals.Dtos.ProposalStateResponseDto>>
        MoveAsync(FakeBudgetActorSource actor, string action, string? remark = null, FakeAuthorizationService? auth = null)
    {
        await using var ctx = _w.NewContext();
        return await TestServices.Transitions(ctx, actor, auth).MoveAsync(_w.WorksProposalAId, action, remark);
    }

    [Fact]
    public async Task BUD031_out_of_scope_proposal_is_NOT_FOUND()
    {
        (await MoveAsync(_w.DivisionB, "SUBMIT")).MessageId.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task BUD235_visible_non_holder_gets_M19()
    {
        (await MoveAsync(_w.Circle, "SUBMIT")).MessageId.Should().Be("M-19", "the Circle sees the Division's proposal but does not hold it");
    }

    [Fact]
    public async Task BUD030_missing_transition_right_gets_M20()
    {
        (await MoveAsync(_w.DivisionA, "SUBMIT", auth: TestServices.Allow("F11.Submit"))).MessageId.Should().Be("M-20");
    }

    [Fact]
    public async Task BUD230_a_move_writes_exactly_one_movement_with_the_client_ip()
    {
        var r = await MoveAsync(_w.DivisionA, "SUBMIT");
        r.IsSucced.Should().BeTrue(r.Message);
        r.Data!.StateCode.Should().Be("S-02");
        r.Data.StateName.Should().Be("Submitted, awaiting Circle");
        r.Data.HolderOfficeId.Should().Be(_w.CircleOfficeId);

        await using var ctx = _w.NewContext();
        var moves = await ctx.ProposalMovements.AsNoTracking().Where(m => m.ProposalId == _w.WorksProposalAId).ToListAsync();
        moves.Should().ContainSingle();
        moves[0].ClientIp.ToString().Should().Be("192.0.2.1");
        moves[0].RoleCode.Should().Be("DDO");
        moves[0].CreatedBy.Should().Be(_w.DivisionA.Actor!.EmployeeId);
        var p = await ctx.BudgetProposals.AsNoTracking().SingleAsync(x => x.ProposalId == _w.WorksProposalAId);
        p.FirstSubmittedAt.Should().NotBeNull("set on the first SUBMIT");
    }

    [Fact]
    public async Task BUD120_RETURN_without_a_remark_gets_M12_and_with_one_reaches_the_Division()
    {
        (await MoveAsync(_w.DivisionA, "SUBMIT")).IsSucced.Should().BeTrue();
        (await MoveAsync(_w.Circle, "OPEN")).IsSucced.Should().BeTrue();

        (await MoveAsync(_w.Circle, "RETURN", "   ")).MessageId.Should().Be("M-12");
        var r = await MoveAsync(_w.Circle, "RETURN", "See observations");
        r.IsSucced.Should().BeTrue(r.Message);
        r.Data!.StateCode.Should().Be("S-04");
        r.Data.HolderOfficeId.Should().Be(_w.DivisionAOfficeId);
    }

    [Fact]
    public async Task BUD214_recall_before_open_and_refused_after_open()
    {
        (await MoveAsync(_w.DivisionA, "SUBMIT")).IsSucced.Should().BeTrue();
        var recalled = await MoveAsync(_w.DivisionA, "RECALL");
        recalled.IsSucced.Should().BeTrue(recalled.Message);
        recalled.Data!.StateCode.Should().Be("S-01");

        (await MoveAsync(_w.DivisionA, "SUBMIT")).IsSucced.Should().BeTrue();
        (await MoveAsync(_w.Circle, "OPEN")).IsSucced.Should().BeTrue();
        (await MoveAsync(_w.DivisionA, "RECALL")).MessageId.Should().Be("M-19");
    }

    [Fact]
    public async Task BUD191_move_refused_when_the_cycle_is_frozen()
    {
        await using (var ctx = _w.NewContext())
        await using (var tx = await new BBMB.BudgetService.Shared.Services.BudgetActors.BudgetActionScope(ctx, _w.ItCell).BeginAsync("TEST"))
        {
            await ctx.Database.ExecuteSqlRawAsync($"UPDATE budget.budget_cycle SET status = 'FROZEN' WHERE cycle_id = {_w.CycleId}");
            await tx.CommitAsync();
        }
        (await MoveAsync(_w.DivisionA, "SUBMIT")).MessageId.Should().Be("M-17");
    }

    [Fact]
    public async Task S1114_without_T16_a_move_returns_M19_not_500()
    {
        var cs = await fixture.CreateFreshDatabaseAsync($"pts_not16_{Guid.NewGuid():N}"[..24]);
        await fixture.MigrateToAsync(cs, "20261014152621_BudgetP1FiguresAndRecommendations");
        await using var ctx = BudgetDatabaseFixture.NewBudgetContext(cs);
        var service = TestServices.Transitions(ctx, _w.DivisionA);

        (await service.IsStateModelAvailableAsync()).Should().BeFalse();
        (await service.MoveAsync(12345, "SUBMIT", null)).MessageId.Should().Be("M-19");
    }
}
