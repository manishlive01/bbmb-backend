using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetBySubMajorHead;
using BBMB.BudgetService.Tests.Common;
using FluentAssertions;

namespace BBMB.BudgetService.Tests.Features.MinorHeads.GetBySubMajorHead;

public class GetMinorHeadsBySubMajorHeadTests : InMemoryTestBase
{
    private static readonly DateTime ModifiedOn = new(2026, 1, 15, 9, 30, 0, DateTimeKind.Utc);

    private Task SeedMinorHeadsAsync()
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
                MinorAccCode = "301",
                MinorAccDesc = "Irrigation Works",
                ModificationDate = ModifiedOn,
                UserCode = "SYSTEM"
            });

        return Context.SaveChangesAsync();
    }

    [Fact]
    public async Task Should_Return_MinorHeads_With_Mapped_Fields()
    {
        // Arrange
        await SeedMinorHeadsAsync();
        var handler = new GetMinorHeadsBySubMajorHeadHandler(Context);

        // Act
        var result = await handler.Handle(new GetMinorHeadsBySubMajorHeadQuery("01"), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Select(x => x.MinorAccCode)
            .Should()
            .ContainInOrder("201", "301");
        result.Select(x => x.MinorAccDesc)
            .Should()
            .BeEquivalentTo(new[] { "Bhakra Power Houses - Power Wing", "Irrigation Works" });
        result.Should().OnlyContain(x => x.UserCode == "SYSTEM");
        result.Should().OnlyContain(x => x.ModificationDate == ModifiedOn);
    }

    [Fact]
    public async Task Should_Return_Empty_List_When_No_MinorHeads_Exist()
    {
        // Arrange
        var handler = new GetMinorHeadsBySubMajorHeadHandler(Context);

        // Act
        var result = await handler.Handle(new GetMinorHeadsBySubMajorHeadQuery("01"), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
