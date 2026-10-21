using BBMB.BudgetService.Features.BudgetProcess.Proposals.AcquireLease;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.GetById;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.GetById.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.GetList;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.GetStateModel;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.ReleaseLease;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace BBMB.BudgetService.Tests.Features.BudgetProcess.Proposals;

/// <summary>Design §4.10 (lease), §8.2 budget-states, §8.3 proposal reads, §11.4 (T-16 absent).</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class ProposalReadAndLeaseTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;
    private static readonly IConfiguration Config = new ConfigurationBuilder().Build(); // default 20 minutes

    public async Task InitializeAsync() => _w = await SpineWorld.CreateAsync(fixture, $"prl_{Guid.NewGuid():N}"[..20]);

    public Task DisposeAsync() => Task.CompletedTask;

    private FakeBudgetActorSource At(long officeId, string level, string role) =>
        new(_w.ItCell.Actor! with { OfficeId = officeId, OfficeLevel = level, RoleCode = role });

    private async Task<Response<LeaseResponseDto>> AcquireAsync(FakeBudgetActorSource actor, params string[] rights)
    {
        await using var ctx = _w.NewContext();
        return await new AcquireLeaseHandler(ctx, actor, new BudgetActionScope(ctx, actor), TestServices.OfficeScope(ctx),
                TestServices.Allow(rights), new HttpContextAccessor { HttpContext = new DefaultHttpContext() }, Config)
            .Handle(new AcquireLeaseCommand(_w.WorksProposalAId), default);
    }

    private static async Task<Response<GetProposalByIdResponseDto>> GetByIdAsync(SpineWorld w, FakeBudgetActorSource actor, long id)
    {
        await using var ctx = w.NewContext();
        return await new GetProposalByIdHandler(ctx, actor, TestServices.OfficeScope(ctx), TestServices.Transitions(ctx, actor))
            .Handle(new GetProposalByIdQuery(id), default);
    }

    private async Task MoveAsync(FakeBudgetActorSource actor, string action)
    {
        await using var ctx = _w.NewContext();
        var r = await TestServices.Transitions(ctx, actor).MoveAsync(_w.WorksProposalAId, action, null);
        r.IsSucced.Should().BeTrue($"{action}: {r.Message}");
    }

    /// <summary>Division → Circle → CE → Secretariat → Finance, ending at S-10 held by Finance.</summary>
    private async Task MoveToS10Async()
    {
        var ce = At(_w.CeOfficeId, "CE", "CE");
        var sec = At(_w.SecretariatOfficeId, "SECRETARIAT", "SECRETARY");
        var fin = At(_w.FinanceOfficeId, "FINANCE", "AFA_BUDGET");
        await MoveAsync(_w.DivisionA, "SUBMIT");
        await MoveAsync(_w.Circle, "OPEN");
        await MoveAsync(_w.Circle, "FORWARD");
        await MoveAsync(ce, "OPEN");
        await MoveAsync(ce, "FORWARD");
        await MoveAsync(sec, "OPEN");
        await MoveAsync(sec, "FORWARD_FINANCE");
        await MoveAsync(fin, "OPEN");
    }

    [Fact]
    public async Task BUD015_FinViewLeaseIsReadOnlyAndDoesNotBlockAfa()
    {
        await MoveToS10Async();
        var finView = new FakeBudgetActorSource((await TestActor.CreateAsync(fixture, "FIN_VIEW", _w.FinanceOfficeId, "FINANCE",
            name: "Section Officer", connectionString: _w.ConnectionString)).Actor);
        var afa = new FakeBudgetActorSource((await TestActor.CreateAsync(fixture, "AFA_BUDGET", _w.FinanceOfficeId, "FINANCE",
            name: "AFA Budget", connectionString: _w.ConnectionString)).Actor);

        var view = await AcquireAsync(finView, "Budget.View");
        view.IsSucced.Should().BeTrue();
        view.Data!.Granted.Should().BeFalse();
        view.Data.ReadOnly.Should().BeTrue();

        var granted = await AcquireAsync(afa, "Budget.View", "F10.Act");
        granted.Data!.Granted.Should().BeTrue("a reader's attempt leaves no lease");
        granted.Data.HeldBy.Should().Be(afa.Actor!.EmployeeId);
        granted.Data.ExpiresAt.Should().BeCloseTo(DateTimeOffset.UtcNow.AddMinutes(20), TimeSpan.FromMinutes(1));

        var viewAgain = await AcquireAsync(finView, "Budget.View");
        viewAgain.Data!.ReadOnly.Should().BeTrue();
        viewAgain.Data.HeldBy.Should().Be(afa.Actor.EmployeeId);
    }

    [Fact]
    public async Task BUD015_second_DDO_of_the_holding_office_gets_M23_while_the_lease_is_live()
    {
        var ddo2 = new FakeBudgetActorSource((await TestActor.CreateAsync(fixture, "DDO", _w.DivisionAOfficeId, "DIVISION",
            name: "Second DDO", connectionString: _w.ConnectionString)).Actor);

        (await AcquireAsync(_w.DivisionA, "F07.Enter")).Data!.Granted.Should().BeTrue();
        var second = await AcquireAsync(ddo2, "F07.Enter");
        second.MessageId.Should().Be("M-23");
        second.Data!.HeldBy.Should().Be(_w.DivisionA.Actor!.EmployeeId);

        await using (var ctx = _w.NewContext())
        {
            (await new ReleaseLeaseHandler(ctx, ddo2, new BudgetActionScope(ctx, ddo2), TestServices.OfficeScope(ctx))
                .Handle(new ReleaseLeaseCommand(_w.WorksProposalAId), default)).MessageId.Should().Be("M-23", "not ddo2's lease");
        }
        await using (var ctx = _w.NewContext())
        {
            (await new ReleaseLeaseHandler(ctx, _w.DivisionA, new BudgetActionScope(ctx, _w.DivisionA), TestServices.OfficeScope(ctx))
                .Handle(new ReleaseLeaseCommand(_w.WorksProposalAId), default)).IsSucced.Should().BeTrue();
        }
        (await AcquireAsync(ddo2, "F07.Enter")).Data!.Granted.Should().BeTrue("released");
    }

    [Fact]
    public async Task BUD015_lease_is_read_only_for_a_visible_non_holder_and_NOT_FOUND_out_of_scope()
    {
        (await AcquireAsync(_w.Circle, "Review.Examine")).Data!.ReadOnly.Should().BeTrue();
        (await AcquireAsync(_w.DivisionB, "F07.Enter")).MessageId.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task BUD031_proposal_list_and_detail_are_scoped()
    {
        async Task<PagedResponse<BBMB.BudgetService.Features.BudgetProcess.Proposals.GetList.Dtos.GetProposalListResponseDto>> ListAs(
            FakeBudgetActorSource actor, bool pending = false)
        {
            await using var ctx = _w.NewContext();
            return await new GetProposalListHandler(ctx, actor, TestServices.OfficeScope(ctx))
                .Handle(new GetProposalListQuery(1, 50, _w.CycleId, null, null, null, pending), default);
        }
        (await ListAs(_w.DivisionB)).Data.Should().NotContain(p => p.ProposalId == _w.WorksProposalAId);
        var mine = (await ListAs(_w.DivisionA)).Data.Should().ContainSingle().Subject;
        mine.StateName.Should().Be("Under preparation");
        mine.OriginOfficeName.Should().NotBeNullOrEmpty();
        (await ListAs(_w.Circle)).Data.Should().Contain(p => p.ProposalId == _w.WorksProposalAId);
        (await ListAs(_w.Circle, pending: true)).Data.Should().BeEmpty("held by the Division");

        (await GetByIdAsync(_w, _w.DivisionB, _w.WorksProposalAId)).MessageId.Should().Be("NOT_FOUND");
        var detail = await GetByIdAsync(_w, _w.DivisionA, _w.WorksProposalAId);
        detail.Data!.AvailableActions.Should().Equal("SUBMIT");
        (await GetByIdAsync(_w, _w.Circle, _w.WorksProposalAId)).Data!.AvailableActions.Should().BeEmpty("the Circle does not hold it");
    }

    [Fact]
    public async Task S114_without_T16_detail_has_no_actions_and_the_state_model_no_transitions()
    {
        var early = await SpineWorld.CreateAsync(fixture, $"prl0_{Guid.NewGuid():N}"[..20],
            migrateTo: "20261014152621_BudgetP1FiguresAndRecommendations");

        var detail = await GetByIdAsync(early, early.DivisionA, early.WorksProposalAId);
        detail.IsSucced.Should().BeTrue(detail.Message);
        detail.Data!.AvailableActions.Should().BeEmpty();

        await using var ctx = early.NewContext();
        var model = await new GetStateModelHandler(ctx, TestServices.Transitions(ctx, early.DivisionA))
            .Handle(new GetStateModelQuery(), default);
        model.Data!.StateModelAvailable.Should().BeFalse();
        model.Data.Transitions.Should().BeEmpty();
        model.Data.States.Should().HaveCount(16);
    }

    [Fact]
    public async Task S82_state_model_lists_the_seeded_states_and_transitions()
    {
        await using var ctx = _w.NewContext();
        var model = await new GetStateModelHandler(ctx, TestServices.Transitions(ctx, _w.DivisionA))
            .Handle(new GetStateModelQuery(), default);
        model.Data!.StateModelAvailable.Should().BeTrue();
        model.Data.Transitions.Should().Contain(t => t.FromState == "S-01" && t.ActionCode == "SUBMIT" && t.ToState == "S-02");
    }
}
