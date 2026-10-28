using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetList;
using BBMB.BudgetService.Tests.Common;
using FluentAssertions;

namespace BBMB.BudgetService.Tests.Features.MinorHeads.GetList;

public class GetMinorHeadListTests : InMemoryTestBase
{
    private static readonly DateTime ModifiedOn = new(2026, 1, 15, 9, 30, 0, DateTimeKind.Utc);

    private static GetMinorHeadListQuery Query(
        int pageNumber = 1,
        int pageSize = 10,
        string? sortBy = null,
        bool sortDescending = false,
        string? minorAccCode = null,
        string? minorAccDesc = null,
        string? userCode = null,
        DateTime? modificationDateFrom = null,
        DateTime? modificationDateTo = null)
        => new(
            PageNumber: pageNumber,
            PageSize: pageSize,
            SortBy: sortBy,
            SortDescending: sortDescending,
            MinorAccCode: minorAccCode,
            MajorAccCode: null,
            SubMajorAccCode: null,
            MinorAccDesc: minorAccDesc,
            UserCode: userCode,
            ModificationDateFrom: modificationDateFrom,
            ModificationDateTo: modificationDateTo);

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
                MinorAccCode = "202",
                MinorAccDesc = "Ganguwal and Kotla Power Houses",
                ModificationDate = ModifiedOn.AddDays(1),
                UserCode = "AFA_BUDGET"
            },
            new MinorHead
            {
                MinorAccCode = "203",
                MinorAccDesc = "Dehar Power House",
                ModificationDate = ModifiedOn.AddDays(2),
                UserCode = "SYSTEM"
            });

        return Context.SaveChangesAsync();
    }

    [Fact]
    public async Task Should_Return_All_MinorHeads_Ordered_By_Code()
    {
        // Arrange
        await SeedMinorHeadsAsync();
        var handler = new GetMinorHeadListHandler(Context);

        // Act
        var result = await handler.Handle(Query(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Be("Minor heads retrieved successfully");
        result.Data.Should().HaveCount(3);
        result.Data.Select(x => x.MinorAccCode)
            .Should()
            .ContainInOrder("201", "202", "203");
        result.Pagination.TotalCount.Should().Be(3);
        result.Pagination.PageNumber.Should().Be(1);
        result.Pagination.PageSize.Should().Be(10);
        result.Pagination.TotalPages.Should().Be(1);
        result.Pagination.HasPreviousPage.Should().BeFalse();
        result.Pagination.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task Should_Return_Empty_List_When_No_MinorHeads_Exist()
    {
        // Arrange
        var handler = new GetMinorHeadListHandler(Context);

        // Act
        var result = await handler.Handle(Query(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeEmpty();
        result.Pagination.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Should_Filter_By_MinorAccCode()
    {
        // Arrange
        await SeedMinorHeadsAsync();
        var handler = new GetMinorHeadListHandler(Context);

        // Act
        var result = await handler.Handle(Query(minorAccCode: "202"), CancellationToken.None);

        // Assert
        result.Data.Should().ContainSingle();
        result.Data[0].MinorAccCode.Should().Be("202");
        result.Data[0].MinorAccDesc.Should().Be("Ganguwal and Kotla Power Houses");
        result.Pagination.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Should_Filter_By_MinorAccDesc()
    {
        // Arrange
        await SeedMinorHeadsAsync();
        var handler = new GetMinorHeadListHandler(Context);

        // Act
        var result = await handler.Handle(Query(minorAccDesc: "Dehar"), CancellationToken.None);

        // Assert
        result.Data.Should().ContainSingle();
        result.Data[0].MinorAccCode.Should().Be("203");
    }

    [Fact]
    public async Task Should_Filter_By_UserCode()
    {
        // Arrange
        await SeedMinorHeadsAsync();
        var handler = new GetMinorHeadListHandler(Context);

        // Act
        var result = await handler.Handle(Query(userCode: "AFA_BUDGET"), CancellationToken.None);

        // Assert
        result.Data.Should().ContainSingle();
        result.Data[0].MinorAccCode.Should().Be("202");
        result.Data[0].UserCode.Should().Be("AFA_BUDGET");
    }

    [Fact]
    public async Task Should_Filter_By_ModificationDate_Range()
    {
        // Arrange
        await SeedMinorHeadsAsync();
        var handler = new GetMinorHeadListHandler(Context);

        // Act
        var result = await handler.Handle(
            Query(
                modificationDateFrom: ModifiedOn.AddDays(1),
                modificationDateTo: ModifiedOn.AddDays(1)),
            CancellationToken.None);

        // Assert
        result.Data.Should().ContainSingle();
        result.Data[0].MinorAccCode.Should().Be("202");
    }

    [Fact]
    public async Task Should_Page_Results()
    {
        // Arrange
        await SeedMinorHeadsAsync();
        var handler = new GetMinorHeadListHandler(Context);

        // Act
        var result = await handler.Handle(Query(pageNumber: 2, pageSize: 1), CancellationToken.None);

        // Assert
        result.Data.Should().ContainSingle();
        result.Data[0].MinorAccCode.Should().Be("202");
        result.Pagination.TotalCount.Should().Be(3);
        result.Pagination.TotalPages.Should().Be(3);
        result.Pagination.HasPreviousPage.Should().BeTrue();
        result.Pagination.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task Should_Sort_Descending_When_Requested()
    {
        // Arrange
        await SeedMinorHeadsAsync();
        var handler = new GetMinorHeadListHandler(Context);

        // Act
        var result = await handler.Handle(
            Query(sortBy: "minoracccode", sortDescending: true),
            CancellationToken.None);

        // Assert
        result.Data.Select(x => x.MinorAccCode)
            .Should()
            .ContainInOrder("203", "202", "201");
    }
}
