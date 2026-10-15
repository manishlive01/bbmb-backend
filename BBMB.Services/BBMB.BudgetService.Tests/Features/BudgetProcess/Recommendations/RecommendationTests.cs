using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Entities;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules;
using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.FinaliseCategory;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.FinaliseCategory.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Recommendations.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Recommendations.GetRows;
using BBMB.BudgetService.Features.BudgetProcess.Recommendations.Upsert;
using BBMB.BudgetService.Features.BudgetProcess.Recommendations.Upsert.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Tests.Features.BudgetProcess.Recommendations;
/// <summary>Design §5.3 T-37 / T-38, §8.3 recommendation rows (BUD-153, BUD-302, BUD-303, BUD-304).</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class RecommendationTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;
    private long _dna;
    public async Task InitializeAsync()
    {
        _w = await SpineWorld.CreateAsync(fixture, $"rec_{Guid.NewGuid():N}"[..20]);
        await using var ctx = _w.NewContext();
        await using (var tx = await new BudgetActionScope(ctx, _w.ItCell).BeginAsync("OPEN_PROPOSALS"))
        {
            var p = new BudgetProposal { CycleId = _w.CycleId, Category = "DNA", OriginOfficeId = _w.DivisionAOfficeId, HolderOfficeId = _w.DivisionAOfficeId, StateCode = "S-01" };
            ctx.BudgetProposals.Add(p);
            await ctx.SaveChangesAsync();
            await tx.CommitAsync();
            _dna = p.ProposalId;
        }
        await using var lctx = _w.NewContext();
        await using var ltx = await new BudgetActionScope(lctx, _w.DivisionA).BeginAsync("DEMAND_LINE_ADD");
        lctx.DemandLines.Add(new DemandLine { ProposalId = _dna, LineType = "HEAD", AccountCodeId = _w.AccountCodeId, SerialNo = 1, RbeAmount = 10, ObeNextAmount = 20 });
        await lctx.SaveChangesAsync();
        await ltx.CommitAsync();
        // From P2 the head-wise expected set is every office with a live T-21 assignment (design §6.2, OI-43). This
        // test concerns one Division's DNA demand, so the world's other assignments are removed (by marking).
        await using var actx = _w.NewContext();
        await using var atx = await new BudgetActionScope(actx, _w.ItCell).BeginAsync("REMOVE_HEAD");
        await actx.Database.ExecuteSqlRawAsync(
            $"UPDATE budget.account_head_assignment SET removed_at = now() WHERE office_id <> {_w.DivisionAOfficeId}");
        await atx.CommitAsync();
    }
    public Task DisposeAsync() => Task.CompletedTask;
    private FakeBudgetActorSource At(long officeId, string level, string role) =>
        new(_w.ItCell.Actor! with { OfficeId = officeId, OfficeLevel = level, RoleCode = role });
    private FakeBudgetActorSource Fin => At(_w.FinanceOfficeId, "FINANCE", "AFA_BUDGET");

    private async Task MoveToAsync(string upTo)
    {
        var ce = At(_w.CeOfficeId, "CE", "CE");
        var sec = At(_w.SecretariatOfficeId, "SECRETARIAT", "SECRETARY");
        var steps = new (FakeBudgetActorSource, string, string)[]
        {
            (_w.DivisionA, "SUBMIT", "S-02"), (_w.Circle, "OPEN", "S-03"), (_w.Circle, "FORWARD", "S-05"), (ce, "OPEN", "S-06"),
            (ce, "FORWARD", "S-07"), (sec, "OPEN", "S-08"), (sec, "FORWARD_FINANCE", "S-09"), (Fin, "OPEN", "S-10"),
        };
        foreach (var (actor, action, reached) in steps)
        {
            await using var ctx = _w.NewContext();
            (await TestServices.Transitions(ctx, actor).MoveAsync(_dna, action, null)).IsSucced.Should().BeTrue(action);
            if (reached == upTo) return;
        }
    }

    private async Task<Response<long>> UpsertAsync(decimal? rbe, decimal? obe, string? concurrence, long? ceOfficeId = null)
    {
        await using var ctx = _w.NewContext();
        return await new UpsertRecommendationHandler(ctx, Fin, new BudgetActionScope(ctx, Fin))
            .Handle(new UpsertRecommendationCommand(_w.CycleId, new UpsertRecommendationDto
            {
                Category = "DNA", CeOfficeId = ceOfficeId ?? _w.CeOfficeId, AccountCodeId = _w.AccountCodeId,
                RbeRecommended = rbe, ObeNextRecommended = obe, ConcurrenceRef = concurrence,
            }), default);
    }

    private async Task<Response> FinaliseAsync()
    {
        await using var ctx = _w.NewContext();
        return await new FinaliseCategoryHandler(ctx, Fin, TestServices.AllowAll(), new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
                TestServices.Transitions(ctx, Fin), new DemandLineRules(new DemandLineRuleInputsLoader(ctx)))
            .Handle(new FinaliseCategoryCommand(_w.CycleId, "DNA"), default);
    }

    [Fact]
    public void BUD153_a_figure_without_concurrenceRef_is_400()
    {
        var v = new UpsertRecommendationValidator();
        v.Validate(new UpsertRecommendationCommand(1, new UpsertRecommendationDto { Category = "DNA", CeOfficeId = 1, AccountCodeId = 1, RbeRecommended = 5 }))
            .Errors.Should().Contain(e => e.ErrorCode == "M-21");
        v.Validate(new UpsertRecommendationCommand(1, new UpsertRecommendationDto { Category = "WORKS", CeOfficeId = 1, AccountCodeId = 1 })).IsValid.Should().BeFalse();
        v.Validate(new UpsertRecommendationCommand(1, new UpsertRecommendationDto { Category = "DNA", CeOfficeId = 1, AccountCodeId = 1, RbeRecommended = 5.5m, ConcurrenceRef = "C" }))
            .Errors.Should().Contain(e => e.ErrorCode == "M-22");
    }

    [Fact]
    public async Task BUD303_a_proposal_under_the_CE_at_S08_is_M19_and_a_non_CE_office_is_M22()
    {
        await MoveToAsync("S-08");
        (await UpsertAsync(10, 20, "FA&CAO note 1")).MessageId.Should().Be("M-19");
        (await UpsertAsync(10, 20, "FA&CAO note 1", ceOfficeId: _w.CircleOfficeId)).MessageId.Should().Be("M-22");
    }

    [Fact]
    public async Task BUD302_finalise_is_refused_until_every_demanded_row_has_a_recommendation_and_a_second_upsert_is_version_2()
    {
        await MoveToAsync("S-10");
        var refused = await FinaliseAsync();
        refused.MessageId.Should().Be("M-19");
        ((Response<FinaliseResponseDto>)refused).Data!.MissingRecommendations.Should().Equal(new MissingRecommendationDto(_w.CeOfficeId, _w.AccountCodeId));
        var first = await UpsertAsync(10, 18, "FA&CAO note 1");
        first.IsSucced.Should().BeTrue(first.Message);
        (await FinaliseAsync()).IsSucced.Should().BeTrue();
        (await UpsertAsync(9, 18, "FA&CAO note 2")).IsSucced.Should().BeTrue("allowed at S-11");
        await using var ctx = _w.NewContext();
        (await ctx.RecommendationVersions.AsNoTracking().Where(v => v.RecommendationId == first.Data).OrderBy(v => v.VersionNo)
                .Select(v => new { v.VersionNo, v.RbeRecommended }).ToListAsync())
            .Should().BeEquivalentTo(new[] { new { VersionNo = (short)1, RbeRecommended = (decimal?)10 }, new { VersionNo = (short)2, RbeRecommended = (decimal?)9 } },
                o => o.WithStrictOrdering());
        (await ctx.CeRecommendations.AsNoTracking().CountAsync(r => r.CycleId == _w.CycleId)).Should().Be(1, "one row per (cycle, category, CE, code)");
    }

    [Fact]
    public async Task BUD304_a_CE_sees_only_its_rows_and_Finance_sees_all()
    {
        long ce2;
        await using (var ctx = _w.NewContext())
        await using (var tx = await new BudgetActionScope(ctx, _w.ItCell).BeginAsync("TEST_SETUP"))
        {
            var o = new BudgetOffice { OfficeLevel = "CE", CeId = _w.HierarchyB.CeId, WingCode = "I", ParentOfficeId = _w.SecretariatOfficeId };
            ctx.BudgetOffices.Add(o);
            await ctx.SaveChangesAsync();
            ctx.CeRecommendations.Add(new CeRecommendation { CycleId = _w.CycleId, Category = "DNA", CeOfficeId = o.OfficeId, AccountCodeId = _w.AccountCodeId, RbeRecommended = 1, ConcurrenceRef = "C" });
            await ctx.SaveChangesAsync();
            await tx.CommitAsync();
            ce2 = o.OfficeId;
        }
        await using var rctx = _w.NewContext();
        async Task<List<long>> CesSeenBy(FakeBudgetActorSource a) =>
            (await new GetRecommendationRowsHandler(rctx, a, TestServices.OfficeScope(rctx)).Handle(new GetRecommendationRowsQuery(_w.CycleId, "DNA"), default))
                .Data!.Select(r => r.CeOfficeId).ToList();
        (await CesSeenBy(Fin)).Should().BeEquivalentTo(new[] { _w.CeOfficeId, ce2 });
        (await CesSeenBy(At(_w.CeOfficeId, "CE", "CE"))).Should().Equal(_w.CeOfficeId);
        (await CesSeenBy(_w.Circle)).Should().BeEmpty();
        var row = (await new GetRecommendationRowsHandler(rctx, Fin, TestServices.OfficeScope(rctx)).Handle(new GetRecommendationRowsQuery(_w.CycleId, "DNA"), default))
            .Data!.Single(r => r.CeOfficeId == _w.CeOfficeId);
        (row.RbeDemanded, row.ObeNextDemanded, row.RecommendationId).Should().Be((10m, 20m, (long?)null));
    }
}
