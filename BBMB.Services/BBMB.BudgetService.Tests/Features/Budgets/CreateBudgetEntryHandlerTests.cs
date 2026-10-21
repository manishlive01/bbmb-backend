using BBMB.BudgetService.Features.Budgets.Create;
using BBMB.BudgetService.Features.Budgets.Create.Dtos;
using BBMB.BudgetService.Features.Budgets.Entities;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Tests.Features.Budgets;

/// <summary>Design §3 D-2.</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class CreateBudgetEntryHandlerTests(BudgetDatabaseFixture fixture)
{
    private static CreateBudgetEntryDto Dto(int divisionCode, int seq) => new()
    {
        DivisionCode = divisionCode,
        DivSection = 1,
        CalendarYear = 2026,
        SequenceNo = seq,
        MajorAccCode = "4700",
        SubMajorAccCode = "01",
        MinorAccCode = "001",
        SubMinorAccCode = "01",
        DetailAccCode = "001",
        ObjectAccCode = "001",
        BudgetDescription = "Test",
        BudgetAmount = 100m,
        BudgetType = BudgetType.Work,
        UserCode = "TEST",
    };

    [Fact]
    public async Task D2_CreateBudgetEntryStoresResolvedDivisionId()
    {
        var h = await TestMasters.InsertDivisionAsync(fixture.PlainConnectionString);
        // A second Division whose id equals the first one's code would be linked by the old mapping.
        await TestMasters.InsertDivisionAsync(fixture.PlainConnectionString);

        await using (var ctx = fixture.NewBudgetContext())
        {
            // The handler checks the major head exists.
            await ctx.Database.ExecuteSqlRawAsync(
                "INSERT INTO budget.fa_major_head (major_acc_code, major_acc_desc, modification_date, user_code) " +
                "VALUES ('4700', 'Test major', now(), 'TEST') ON CONFLICT DO NOTHING");
            var response = await new CreateBudgetEntryHandler(ctx).Handle(
                new CreateBudgetEntryCommand(Dto(h.DivisionCode, 9101)), CancellationToken.None);
            response.IsSucced.Should().BeTrue(response.Message);
        }

        await using var read = fixture.NewBudgetContext();
        var entry = await read.BudgetEntries.SingleAsync(e => e.SequenceNo == 9101);
        entry.DivisionId.Should().Be(h.DivisionId);
    }

    [Fact]
    public async Task D2_unknown_division_code_returns_M05_and_saves_nothing()
    {
        await using var ctx = fixture.NewBudgetContext();
        var response = await new CreateBudgetEntryHandler(ctx).Handle(
            new CreateBudgetEntryCommand(Dto(-777, 9102)), CancellationToken.None);
        response.IsSucced.Should().BeFalse();
        response.MessageId.Should().Be("M-05");
        (await ctx.BudgetEntries.AnyAsync(e => e.SequenceNo == 9102)).Should().BeFalse();
    }
}
