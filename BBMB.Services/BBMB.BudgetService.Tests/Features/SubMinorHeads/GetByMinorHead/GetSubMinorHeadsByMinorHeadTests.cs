using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.SubMinor.Entities;
using BBMB.BudgetService.Features.AccountingHeads.SubMinor.GetByMinorHead;
using BBMB.BudgetService.Tests.Common;
using FluentAssertions;

namespace BBMB.BudgetService.Tests.Features.SubMinorHeads.GetByMinorHead;

public class GetSubMinorHeadsByMinorHeadTests : InMemoryTestBase
{
    private static readonly DateTime ModifiedOn = new(2026, 1, 15, 9, 30, 0, DateTimeKind.Utc);

    private Task SeedSubMinorHeadsAsync()
    {
        Context.MinorHeads.AddRange(
            new MinorHead
            {
                MinorAccCode = "201",
                MinorAccDesc = "Bhakra Power Houses - Power Wing",
                ModificationDate = ModifiedOn,
                UserCode = "SYSTEM"
            },
            new MinorHead
            {
                MinorAccCode = "202",
                MinorAccDesc = "Ganguwal and Kotla Power Houses",
                ModificationDate = ModifiedOn,
                UserCode = "SYSTEM"
            });

        Context.SubMinorHeads.AddRange(
            new SubMinorHead
            {
                MinorAccCode = "201",
                SubMinorAccCode = "01",
                SubMinorAccDesc = "Unit No. I",
                ModificationDate = ModifiedOn,
                UserCode = "SYSTEM"
            },
            new SubMinorHead
            {
                MinorAccCode = "201",
                SubMinorAccCode = "02",
                SubMinorAccDesc = "Unit No. II",
                ModificationDate = ModifiedOn,
                UserCode = "SYSTEM"
            },
            new SubMinorHead
            {
                MinorAccCode = "202",
                SubMinorAccCode = "03",
                SubMinorAccDesc = "Unit No. III - Hydel Channel",
                ModificationDate = ModifiedOn,
                UserCode = "SYSTEM"
            });

        return Context.SaveChangesAsync();
    }

    [Fact]
    public async Task Should_Return_SubMinorHeads_For_The_Given_MinorHead()
    {
        // Arrange
        await SeedSubMinorHeadsAsync();
        var handler = new GetSubMinorHeadsByMinorHeadHandler(Context);

        // Act
        var result = await handler.Handle(new GetSubMinorHeadsByMinorHeadQuery("201"), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Select(x => x.SubMinorAccCode)
            .Should()
            .ContainInOrder("01", "02");
        result.Should().OnlyContain(x => x.MinorAccCode == "201");
        result.Should().OnlyContain(x => x.MinorAccDesc == "Bhakra Power Houses - Power Wing");
    }

    [Fact]
    public async Task Should_Return_Empty_List_When_MinorHead_Has_No_SubMinorHeads()
    {
        // Arrange
        await SeedSubMinorHeadsAsync();
        var handler = new GetSubMinorHeadsByMinorHeadHandler(Context);

        // Act
        var result = await handler.Handle(new GetSubMinorHeadsByMinorHeadQuery("999"), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
