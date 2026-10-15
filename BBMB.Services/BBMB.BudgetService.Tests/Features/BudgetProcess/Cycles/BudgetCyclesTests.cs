using BBMB.BudgetService.Features.BudgetProcess.Cycles.Create;
using BBMB.BudgetService.Features.BudgetProcess.Cycles.Create.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Cycles.GetCurrent;
using BBMB.BudgetService.Features.BudgetProcess.Cycles.OpenProposals;
using BBMB.BudgetService.Features.BudgetProcess.Cycles.OpenProposals.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Tests.Features.BudgetProcess.Cycles;

/// <summary>Design §5.3 (T-14, current cycle), §6.2 expected set, §8.2 cycle rows.</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class BudgetCyclesTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;

    public async Task InitializeAsync() => _w = await SpineWorld.CreateAsync(fixture, $"cyc_{Guid.NewGuid():N}"[..20]);

    public Task DisposeAsync() => Task.CompletedTask;

    private static async Task<Response<BBMB.BudgetService.Features.BudgetProcess.Cycles.Dtos.BudgetCycleResponseDto>> CurrentAsync(SpineWorld w)
    {
        await using var ctx = w.NewContext();
        return await new GetCurrentBudgetCycleHandler(ctx).Handle(new GetCurrentBudgetCycleQuery(), default);
    }

    private async Task<Response<long>> CreateAsync(int year, bool first = false)
    {
        await using var ctx = _w.NewContext();
        return await new CreateBudgetCycleHandler(ctx, _w.ItCell, new BudgetActionScope(ctx, _w.ItCell))
            .Handle(new CreateBudgetCycleCommand(new CreateBudgetCycleDto { CurrentFinYear = year, IsFirstCycle = first }), default);
    }

    private async Task<Response<OpenProposalsResponseDto>> OpenAsync(long cycleId, string[] categories, long[]? officeIds = null)
    {
        await using var ctx = _w.NewContext();
        return await new OpenProposalsHandler(ctx, _w.ItCell, new BudgetActionScope(ctx, _w.ItCell))
            .Handle(new OpenProposalsCommand(cycleId, new OpenProposalsDto { Categories = [.. categories], OfficeIds = officeIds?.ToList() }), default);
    }

    private async Task FreezeFirstCycleAsync()
    {
        await using var ctx = _w.NewContext();
        await using var tx = await new BudgetActionScope(ctx, _w.ItCell).BeginAsync("TEST");
        await ctx.Database.ExecuteSqlRawAsync($"UPDATE budget.budget_cycle SET status = 'FROZEN' WHERE cycle_id = {_w.CycleId}");
        await tx.CommitAsync();
    }

    [Fact]
    public async Task S53_current_cycle_with_no_T14_row_is_NOT_FOUND()
    {
        var empty = await SpineWorld.CreateAsync(fixture, $"cyc0_{Guid.NewGuid():N}"[..20], withCycle: false);
        (await CurrentAsync(empty)).MessageId.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task S53_FROZEN_year_N_and_OPEN_year_N_plus_1_gives_N_plus_1()
    {
        await FreezeFirstCycleAsync();
        (await CreateAsync(2027)).IsSucced.Should().BeTrue();
        var r = await CurrentAsync(_w);
        r.IsSucced.Should().BeTrue();
        r.Data!.CurrentFinYear.Should().Be(2027);
        r.Data.NextFinYear.Should().Be(2028);
        r.Data.IsFirstCycle.Should().BeFalse();
    }

    [Fact]
    public async Task S82_create_refuses_missing_years_a_second_cycle_for_a_year_and_a_second_first_cycle()
    {
        (await CreateAsync(2030)).MessageId.Should().Be("M-05", "fa_financial_years has no 2030/2031");
        (await CreateAsync(2026)).MessageId.Should().Be("M-19", "one cycle per current year");
        (await CreateAsync(2027, first: true)).MessageId.Should().Be("M-19", "one first cycle");
        (await CreateAsync(2027)).IsSucced.Should().BeTrue();
    }

    [Fact]
    public async Task S62_open_proposals_twice_creates_each_WORKS_proposal_once_for_every_active_Division()
    {
        var first = await OpenAsync(_w.CycleId, ["WORKS"]);
        first.IsSucced.Should().BeTrue(first.Message);
        first.Data!.Created.Should().ContainSingle().Which.OfficeId.Should().Be(_w.DivisionBOfficeId);
        first.Data.ExistingCount.Should().Be(1, "Division A's proposal already exists");

        var second = await OpenAsync(_w.CycleId, ["WORKS"]);
        second.Data!.Created.Should().BeEmpty();
        second.Data.ExistingCount.Should().Be(2);

        await using var ctx = _w.NewContext();
        var works = await ctx.BudgetProposals.AsNoTracking().Where(p => p.CycleId == _w.CycleId && p.Category == "WORKS").ToListAsync();
        works.Select(p => p.OriginOfficeId).Should().BeEquivalentTo(new[] { _w.DivisionAOfficeId, _w.DivisionBOfficeId });
        works.Should().OnlyContain(p => p.StateCode == "S-01" && p.HolderOfficeId == p.OriginOfficeId);
        (await ctx.AuditLog.AsNoTracking().CountAsync(a => a.TableName == "proposal" && a.Operation == "I" && a.ActionCode == "OPEN_PROPOSALS"))
            .Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task S62_concurrent_open_proposals_creates_each_WORKS_proposal_once()
    {
        var results = await Task.WhenAll(OpenAsync(_w.CycleId, ["WORKS"]), OpenAsync(_w.CycleId, ["WORKS"]));
        results.Should().OnlyContain(r => r.IsSucced, "the table lock serialises the second open behind the first");
        results.Sum(r => r.Data!.Created.Count).Should().Be(1, "Division B's proposal is created exactly once");
        await using var ctx = _w.NewContext();
        (await ctx.BudgetProposals.AsNoTracking().CountAsync(p => p.CycleId == _w.CycleId && p.Category == "WORKS")).Should().Be(2);
    }

    [Fact]
    public void S82_cycle_and_proposal_unique_indexes_are_mapped_to_M19()
    {
        foreach (var c in new[] { "IX_budget_cycle_current_fin_year", "IX_budget_cycle_is_first_cycle", "IX_proposal_cycle_id_category_origin_office_id" })
            BBMB.BudgetService.Shared.Messages.BudgetMessages.UniqueConstraintMessageFor(c).Should().Be("M-19", c);
    }

    [Fact]
    public async Task DB01_head_wise_proposal_for_a_FINANCE_office_is_refused_and_a_Circle_is_opened()
    {
        (await OpenAsync(_w.CycleId, ["DNA"], [_w.FinanceOfficeId])).MessageId.Should().Be("M-22");
        await using (var ctx = _w.NewContext())
            (await ctx.BudgetProposals.AnyAsync(p => p.Category == "DNA")).Should().BeFalse("nothing is created");

        var r = await OpenAsync(_w.CycleId, ["DNA"], [_w.CircleOfficeId, _w.CeOfficeId]);
        // From P2 the expected set also holds every office with a live T-21 assignment (the world assigns its code to
        // the CE, the Circle and both Divisions, design §6.2, OI-43).
        r.Data!.Created.Select(c => c.OfficeId).Should().BeEquivalentTo(
            new[] { _w.CircleOfficeId, _w.CeOfficeId, _w.DivisionAOfficeId, _w.DivisionBOfficeId });
    }

    [Fact]
    public async Task BUD191_open_proposals_in_a_frozen_cycle_gets_M17_and_unknown_cycle_NOT_FOUND()
    {
        (await OpenAsync(987654, ["WORKS"])).MessageId.Should().Be("NOT_FOUND");
        await FreezeFirstCycleAsync();
        (await OpenAsync(_w.CycleId, ["WORKS"])).MessageId.Should().Be("M-17");
    }

    [Fact]
    public void P1_category_RECEIPTS_is_refused_by_the_validator()
    {
        var v = new OpenProposalsValidator();
        v.Validate(new OpenProposalsCommand(1, new OpenProposalsDto { Categories = ["RECEIPTS"] })).IsValid.Should().BeFalse();
        v.Validate(new OpenProposalsCommand(1, new OpenProposalsDto { Categories = [] })).IsValid.Should().BeFalse();
        v.Validate(new OpenProposalsCommand(1, new OpenProposalsDto { Categories = ["DNA"], OfficeIds = [0] })).IsValid.Should().BeFalse();
        v.Validate(new OpenProposalsCommand(1, new OpenProposalsDto { Categories = ["WORKS", "DNA"] })).IsValid.Should().BeTrue();
    }
}
