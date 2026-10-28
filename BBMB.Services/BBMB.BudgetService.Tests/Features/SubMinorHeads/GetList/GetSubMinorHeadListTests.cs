using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.SubMinor.Entities;
using BBMB.BudgetService.Features.AccountingHeads.SubMinor.GetList;
using BBMB.BudgetService.Tests.Common;
using FluentAssertions;

namespace BBMB.BudgetService.Tests.Features.SubMinorHeads.GetList;

public class GetSubMinorHeadListTests : InMemoryTestBase
{
    private static readonly DateTime ModifiedOn = new(2026, 1, 15, 9, 30, 0, DateTimeKind.Utc);

    private static GetSubMinorHeadListQuery Query(
        string? subMinorAccCode = null,
        string? minorAccCode = null,
        string? subMinorAccDesc = null,
        string? userCode = null,
        DateTime? modificationDateFrom = null,
        DateTime? modificationDateTo = null,
        string? sortBy = null,
        bool sortDescending = false,
        int pageNumber = 1,
        int pageSize = 10)
        => new(
            SubMinorAccCode: subMinorAccCode,
            MinorAccCode: minorAccCode,
            SubMinorAccDesc: subMinorAccDesc,
            UserCode: userCode,
            ModificationDateFrom: modificationDateFrom,
            ModificationDateTo: modificationDateTo,
            SortBy: sortBy,
            SortDescending: sortDescending,
            PageNumber: pageNumber,
            PageSize: pageSize);

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
                ModificationDate = ModifiedOn.AddDays(1),
                UserCode = "AFA_BUDGET"
            },
            new SubMinorHead
            {
                MinorAccCode = "202",
                SubMinorAccCode = "03",
                SubMinorAccDesc = "Unit No. III - Hydel Channel",
                ModificationDate = ModifiedOn.AddDays(2),
                UserCode = "SYSTEM"
            });

        return Context.SaveChangesAsync();
    }

    [Fact]
    public async Task Should_Return_All_SubMinorHeads_Ordered_By_Code()
    {
        // Arrange
        await SeedSubMinorHeadsAsync();
        var handler = new GetSubMinorHeadListHandler(Context);

        // Act
        var result = await handler.Handle(Query(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Be("Sub-minor heads retrieved successfully");
        result.Data.Should().HaveCount(3);
        result.Data.Select(x => x.SubMinorAccCode)
            .Should()
            .ContainInOrder("01", "02", "03");
        result.Pagination.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task Should_Return_Empty_List_When_No_SubMinorHeads_Exist()
    {
        // Arrange
        var handler = new GetSubMinorHeadListHandler(Context);

        // Act
        var result = await handler.Handle(Query(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeEmpty();
        result.Pagination.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Should_Filter_By_SubMinorAccCode()
    {
        // Arrange
        await SeedSubMinorHeadsAsync();
        var handler = new GetSubMinorHeadListHandler(Context);

        // Act
        var result = await handler.Handle(Query(subMinorAccCode: "02"), CancellationToken.None);

        // Assert
        result.Data.Should().ContainSingle();
        result.Data[0].SubMinorAccCode.Should().Be("02");
        result.Data[0].SubMinorAccDesc.Should().Be("Unit No. II");
        result.Data[0].MinorAccCode.Should().Be("201");
        result.Data[0].MinorAccDesc.Should().Be("Bhakra Power Houses - Power Wing");
    }

    [Fact]
    public async Task Should_Filter_By_MinorAccCode()
    {
        // Arrange
        await SeedSubMinorHeadsAsync();
        var handler = new GetSubMinorHeadListHandler(Context);

        // Act
        var result = await handler.Handle(Query(minorAccCode: "201"), CancellationToken.None);

        // Assert
        result.Data.Should().HaveCount(2);
        result.Data.Should().OnlyContain(x => x.MinorAccCode == "201");
    }

    [Fact]
    public async Task Should_Filter_By_SubMinorAccDesc()
    {
        // Arrange
        await SeedSubMinorHeadsAsync();
        var handler = new GetSubMinorHeadListHandler(Context);

        // Act
        var result = await handler.Handle(Query(subMinorAccDesc: "Hydel Channel"), CancellationToken.None);

        // Assert
        result.Data.Should().ContainSingle();
        result.Data[0].SubMinorAccCode.Should().Be("03");
    }

    [Fact]
    public async Task Should_Filter_By_UserCode()
    {
        // Arrange
        await SeedSubMinorHeadsAsync();
        var handler = new GetSubMinorHeadListHandler(Context);

        // Act
        var result = await handler.Handle(Query(userCode: "AFA_BUDGET"), CancellationToken.None);

        // Assert
        result.Data.Should().ContainSingle();
        result.Data[0].SubMinorAccCode.Should().Be("02");
    }

    [Fact]
    public async Task Should_Filter_By_ModificationDate_Range()
    {
        // Arrange
        await SeedSubMinorHeadsAsync();
        var handler = new GetSubMinorHeadListHandler(Context);

        // Act
        var result = await handler.Handle(
            Query(
                modificationDateFrom: ModifiedOn.AddDays(2),
                modificationDateTo: ModifiedOn.AddDays(2)),
            CancellationToken.None);

        // Assert
        result.Data.Should().ContainSingle();
        result.Data[0].SubMinorAccCode.Should().Be("03");
    }

    [Fact]
    public async Task Should_Page_Results()
    {
        // Arrange
        await SeedSubMinorHeadsAsync();
        var handler = new GetSubMinorHeadListHandler(Context);

        // Act
        var result = await handler.Handle(Query(pageNumber: 2, pageSize: 1), CancellationToken.None);

        // Assert
        result.Data.Should().ContainSingle();
        result.Data[0].SubMinorAccCode.Should().Be("02");
        result.Pagination.TotalCount.Should().Be(3);
        result.Pagination.TotalPages.Should().Be(3);
        result.Pagination.HasPreviousPage.Should().BeTrue();
        result.Pagination.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task Should_Sort_Descending_When_Requested()
    {
        // Arrange
        await SeedSubMinorHeadsAsync();
        var handler = new GetSubMinorHeadListHandler(Context);

        // Act
        var result = await handler.Handle(
            Query(sortBy: "subminoracccode", sortDescending: true),
            CancellationToken.None);

        // Assert
        result.Data.Select(x => x.SubMinorAccCode)
            .Should()
            .ContainInOrder("03", "02", "01");
    }
}
