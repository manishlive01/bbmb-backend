using BBMB.BudgetService.Features.Budgets.Create;
using BBMB.BudgetService.Features.Budgets.Create.Dtos;
using BBMB.BudgetService.Features.Budgets.Entities;
using BBMB.BudgetService.Features.Budgets.Update;
using BBMB.BudgetService.Features.Budgets.Update.Dtos;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Tests.Features.Budgets;

/// <summary>
/// P4-03 (design §4.6 "API and code changes", §9.3 Budgets P4). After the work-code reset every work code is
/// null, so the legacy budget-entry routes reference the work by its stable id (WorkId) instead of the old
/// int code. Create and Update by WorkId still succeed; the entry links to the work by id.
/// </summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class LegacyBudgetEntryP4Tests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;

    public async Task InitializeAsync() => _w = await SpineWorld.CreateAsync(fixture, $"lb_{Guid.NewGuid():N}"[..20]);

    public Task DisposeAsync() => Task.CompletedTask;

    private CreateBudgetEntryDto CreateDto(int seq, int? workId) => new()
    {
        DivisionCode = _w.HierarchyA.DivisionCode,
        DivSection = 1,
        CalendarYear = 2026,
        SequenceNo = seq,
        MajorAccCode = _w.Segments.Major,
        SubMajorAccCode = _w.Segments.SubMajor,
        MinorAccCode = _w.Segments.Minor,
        SubMinorAccCode = _w.Segments.SubMinor,
        DetailAccCode = _w.Segments.Detail,
        ObjectAccCode = _w.Segments.Object,
        WorkId = workId,
        BudgetDescription = "Legacy entry",
        BudgetAmount = 100m,
        BudgetType = BudgetType.Work,
        UserCode = "T",
    };

    [Fact]
    public async Task Create_legacy_budget_entry_by_workId_after_the_reset_succeeds()
    {
        // The major head the handler checks must exist.
        await using (var seed = _w.NewContext())
            await seed.Database.ExecuteSqlRawAsync(
                $"INSERT INTO budget.fa_major_head (major_acc_code, major_acc_desc, modification_date, user_code) " +
                $"VALUES ('{_w.Segments.Major}', 'M', now(), 'T') ON CONFLICT DO NOTHING");

        await using (var ctx = _w.NewContext())
        {
            var response = await new CreateBudgetEntryHandler(ctx).Handle(
                new CreateBudgetEntryCommand(CreateDto(7301, _w.WorkAId)), default);
            response.IsSucced.Should().BeTrue(response.Message);
        }

        await using var read = _w.NewContext();
        var entry = await read.BudgetEntries.AsNoTracking().SingleAsync(e => e.SequenceNo == 7301);
        entry.WorkId.Should().Be(_w.WorkAId, "the entry links to the work by its stable id, not a work code");
    }

    [Fact]
    public async Task Create_with_unknown_workId_is_refused()
    {
        await using (var seed = _w.NewContext())
            await seed.Database.ExecuteSqlRawAsync(
                $"INSERT INTO budget.fa_major_head (major_acc_code, major_acc_desc, modification_date, user_code) " +
                $"VALUES ('{_w.Segments.Major}', 'M', now(), 'T') ON CONFLICT DO NOTHING");

        await using var ctx = _w.NewContext();
        var response = await new CreateBudgetEntryHandler(ctx).Handle(
            new CreateBudgetEntryCommand(CreateDto(7302, 999999)), default);
        response.IsSucced.Should().BeFalse();
        (await ctx.BudgetEntries.AnyAsync(e => e.SequenceNo == 7302)).Should().BeFalse();
    }

    [Fact]
    public async Task Update_legacy_budget_entry_by_workId_after_the_reset_succeeds()
    {
        await using (var seed = _w.NewContext())
            await seed.Database.ExecuteSqlRawAsync(
                $"INSERT INTO budget.fa_major_head (major_acc_code, major_acc_desc, modification_date, user_code) " +
                $"VALUES ('{_w.Segments.Major}', 'M', now(), 'T') ON CONFLICT DO NOTHING");

        int id;
        await using (var ctx = _w.NewContext())
        {
            await new CreateBudgetEntryHandler(ctx).Handle(
                new CreateBudgetEntryCommand(CreateDto(7303, null)), default);
            id = (await ctx.BudgetEntries.AsNoTracking().SingleAsync(e => e.SequenceNo == 7303)).Id;
        }

        await using (var ctx = _w.NewContext())
        {
            var dto = new UpdateBudgetEntryDto
            {
                DivisionId = _w.HierarchyA.DivisionId,
                DivSectionId = 1,
                CalendarYear = 2026,
                SequenceNo = 7303,
                MajorAccCode = _w.Segments.Major,
                SubMajorAccCode = _w.Segments.SubMajor,
                MinorAccCode = _w.Segments.Minor,
                SubMinorAccCode = _w.Segments.SubMinor,
                DetailAccCode = _w.Segments.Detail,
                ObjectAccCode = _w.Segments.Object,
                WorkId = _w.WorkAId,
                BudgetDescription = "Updated",
                BudgetAmount = 200m,
                BudgetType = BudgetType.Work,
                UserCode = "T",
            };
            var response = await new UpdateBudgetEntryHandler(ctx).Handle(new UpdateBudgetEntryCommand(id, dto), default);
            response.IsSucced.Should().BeTrue(response.Message);
        }

        await using var read = _w.NewContext();
        var entry = await read.BudgetEntries.AsNoTracking().SingleAsync(e => e.Id == id);
        entry.WorkId.Should().Be(_w.WorkAId);
        entry.BudgetDescription.Should().Be("Updated");
    }
}
