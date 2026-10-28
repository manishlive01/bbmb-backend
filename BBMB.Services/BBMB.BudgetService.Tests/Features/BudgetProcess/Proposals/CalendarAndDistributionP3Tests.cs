using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Entities;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules;
using BBMB.BudgetService.Features.BudgetProcess.Distributions.Rules;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Submit;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Tests.Features.BudgetProcess.Proposals;

/// <summary>
/// P7-02 (design §12 P3 exit; RTM backend rows TC-BUD-BE-*). The P3-exit test sweep. The bullets map as:
/// VR-15 not applied to RECEIPTS = DemandLineRuleEvaluatorTests.BUD318_VR15_is_not_applied_to_RECEIPTS (already
/// present); DV-08 and "distribution over on one head but within the total refused" = DV08 unit test below over
/// DistributionRules (the per-recommendation VR-07/DB-18 ceiling, M-14); M-26 warns and does not refuse =
/// BUD212 test below. The full mapping is recorded in budget-backend-api-progress.json (item P7-02 evidence).
///
/// Method names embed the DV/M/BUD token so scripts/generate_rtm_bud.py auto-links each to its RTM test-case id.
/// </summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class CalendarAndDistributionP3Tests(BudgetDatabaseFixture fixture)
{
    // ----------------------------------------------------------------- M-26 (BUD-212), calendar lapse
    private sealed class CalendarWorld : IAsyncLifetime
    {
        public SpineWorld W = default!;
        private readonly BudgetDatabaseFixture _fixture;
        public CalendarWorld(BudgetDatabaseFixture f) => _fixture = f;
        public async Task InitializeAsync() => W = await SpineWorld.CreateAsync(_fixture, $"cal_{System.Guid.NewGuid():N}"[..20]);
        public Task DisposeAsync() => Task.CompletedTask;
    }

    private async Task SeedDivisionLineAsync(SpineWorld w)
    {
        await using var ctx = w.NewContext();
        await using var tx = await new BudgetActionScope(ctx, w.DivisionA).BeginAsync("DEMAND_LINE_ADD");
        var serial = 1;
        ctx.DemandLines.Add(new DemandLine
        {
            ProposalId = w.WorksProposalAId, LineType = "WORK", WorkId = w.WorkAId, AccountCodeId = w.AccountCodeId,
            SerialNo = serial, RbeAmount = 1, ObeNextAmount = 1,
        });
        await ctx.SaveChangesAsync();
        await tx.CommitAsync();
    }

    private async Task SeedCalendarAsync(SpineWorld w, string officeLevel, DateOnly lastDate)
    {
        await using var ctx = w.NewContext();
        await using var tx = await new BudgetActionScope(ctx, w.ItCell).BeginAsync("CALENDAR_SET");
        // budget.calendar is keyed by (cycle_id, office_level); last_date drives the M-26 lapse warning.
        await ctx.Database.ExecuteSqlRawAsync(
            "INSERT INTO budget.calendar (cycle_id, office_level, last_date) VALUES ({0}, {1}, {2})",
            w.CycleId, officeLevel, lastDate);
        await tx.CommitAsync();
    }

    private async Task<Response> SubmitAsync(SpineWorld w)
    {
        await using var ctx = w.NewContext();
        var svc = TestServices.Transitions(ctx, w.DivisionA);
        return await new SubmitProposalHandler(ctx, w.DivisionA, svc, new DemandLineRules(new DemandLineRuleInputsLoader(ctx)))
            .Handle(new SubmitProposalCommand(w.WorksProposalAId), default);
    }

    [Fact]
    public async Task BUD212_M26_a_passed_calendar_date_warns_on_the_move_and_refuses_nothing()
    {
        var world = new CalendarWorld(fixture);
        await world.InitializeAsync();
        var w = world.W;
        await SeedDivisionLineAsync(w);
        await SeedCalendarAsync(w, "DIVISION", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))); // yesterday

        var r = await SubmitAsync(w);

        r.IsSucced.Should().BeTrue("a calendar lapse warns, it does not refuse the move (M-26, BUD-212)");
        var state = ((Response<ProposalStateResponseDto>)r).Data!;
        state.StateCode.Should().Be("S-02", "the submit still happened");
        state.Warnings.Should().Contain(BudgetMessages.M26);
    }

    [Fact]
    public async Task BUD212_no_calendar_entry_adds_no_M26_warning()
    {
        var world = new CalendarWorld(fixture);
        await world.InitializeAsync();
        var w = world.W;
        await SeedDivisionLineAsync(w);
        // No calendar entry for DIVISION: the move carries no M-26.

        var r = await SubmitAsync(w);

        r.IsSucced.Should().BeTrue();
        var state = ((Response<ProposalStateResponseDto>)r).Data!;
        state.StateCode.Should().Be("S-02");
        state.Warnings.Should().NotContain(BudgetMessages.M26);
    }

    // ----------------------------------------------------------------- DV-08, VR-07 distribution ceiling
    // The ceiling is per recommendation (per head): the distribution of ONE head may not exceed that head's
    // recommended figure, column by column, even if the total across heads is within the total recommended.
    // DistributionRules is the pure VR-07/DB-18 check (M-14); the DB re-checks the same set at commit.

    [Fact]
    public void DV08_distribution_over_on_one_column_is_refused_even_when_the_other_column_is_within()
    {
        // Head (recommendation) recommended RBE 1000, OBE 1000. Distribute RBE 1200 (over) and OBE 800 (within).
        // One column over fails VR-07 for this head, regardless of the other column's slack.
        DistributionRules.WithinCeiling(rbeRecommended: 1000, obeNextRecommended: 1000, rbeTotal: 1200, obeTotal: 800)
            .Should().BeFalse();
        DistributionRules.OverColumn(1000, 1000, 1200, 800).Should().Be("rbeAmount");
    }

    [Fact]
    public void DV08_one_head_over_is_refused_though_the_across_heads_total_is_within_the_total_recommended()
    {
        // Two heads: A recommended 1000, B recommended 1000 (total recommended 2000). Distribute A = 1200 and
        // B = 700 (across-heads total 1900 <= 2000). The ceiling is per head, so A's 1200 > 1000 is refused;
        // the within-total-across-heads slack does not rescue head A (DV-08).
        DistributionRules.WithinCeiling(1000, 1000, rbeTotal: 1200, obeTotal: 0).Should().BeFalse("head A is over its own recommended figure");
        DistributionRules.WithinCeiling(1000, 1000, rbeTotal: 700, obeTotal: 0).Should().BeTrue("head B alone is within its figure");
        // A naive across-heads check (1200 + 700 = 1900 <= 2000) would wrongly pass; the per-head rule does not.
        (1200m + 700m <= 2000m).Should().BeTrue("the across-heads total is within — which is exactly why the per-head rule is needed");
    }

    [Fact]
    public void DV08_exactly_at_the_recommended_figure_is_accepted_and_one_rupee_over_is_refused()
    {
        DistributionRules.WithinCeiling(1000, 1000, 1000, 1000).Should().BeTrue("exactly at the recommended figure is within");
        DistributionRules.WithinCeiling(1000, 1000, 1001, 1000).Should().BeFalse("one rupee over on a column is refused");
        DistributionRules.OverColumn(1000, 1000, 1000, 1001).Should().Be("obeNextAmount");
        // A null recommended figure is nil: nothing may be distributed from an unset figure (BUD-157).
        DistributionRules.WithinCeiling(null, null, 1, 0).Should().BeFalse();
    }
}
