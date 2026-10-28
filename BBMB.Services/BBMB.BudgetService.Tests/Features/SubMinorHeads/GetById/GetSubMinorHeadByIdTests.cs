using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.SubMinor.Entities;
using BBMB.BudgetService.Features.AccountingHeads.SubMinor.GetById;
using BBMB.BudgetService.Tests.Common;
using FluentAssertions;

namespace BBMB.BudgetService.Tests.Features.SubMinorHeads.GetById;

public class GetSubMinorHeadByIdTests : InMemoryTestBase
{
    private static readonly DateTime ModifiedOn = new(2026, 1, 15, 9, 30, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Should_Return_SubMinorHead_When_Code_Exists()
    {
        // Arrange
        Context.MinorHeads.Add(new MinorHead
        {
            MinorAccCode = "201",
            MinorAccDesc = "Bhakra Power Houses - Power Wing",
            ModificationDate = ModifiedOn,
            UserCode = "SYSTEM"
        });
        Context.SubMinorHeads.Add(new SubMinorHead
        {
            MinorAccCode = "201",
            SubMinorAccCode = "03",
            SubMinorAccDesc = "Unit No. III - Hydel Channel",
            ModificationDate = ModifiedOn,
            UserCode = "SYSTEM"
        });
        await Context.SaveChangesAsync();

        var handler = new GetSubMinorHeadByIdHandler(Context);

        // Act
        var result = await handler.Handle(new GetSubMinorHeadByIdQuery("03"), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SubMinorAccCode.Should().Be("03");
        result.SubMinorAccDesc.Should().Be("Unit No. III - Hydel Channel");
        result.MinorAccCode.Should().Be("201");
        result.MinorAccDesc.Should().Be("Bhakra Power Houses - Power Wing");
        result.ModificationDate.Should().Be(ModifiedOn);
        result.UserCode.Should().Be("SYSTEM");
    }

    [Fact]
    public async Task Should_Throw_KeyNotFound_When_Code_Does_Not_Exist()
    {
        // Arrange
        var handler = new GetSubMinorHeadByIdHandler(Context);

        // Act
        Func<Task> act = () => handler.Handle(new GetSubMinorHeadByIdQuery("99"), CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("*99*");
    }

    [Fact]
    public async Task Should_Return_The_Matching_SubMinorHead_When_Multiple_Exist()
    {
        // Arrange
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
                MinorAccCode = "202",
                SubMinorAccCode = "02",
                SubMinorAccDesc = "Unit No. II",
                ModificationDate = ModifiedOn,
                UserCode = "SYSTEM"
            });
        await Context.SaveChangesAsync();

        var handler = new GetSubMinorHeadByIdHandler(Context);

        // Act
        var result = await handler.Handle(new GetSubMinorHeadByIdQuery("02"), CancellationToken.None);

        // Assert
        result.SubMinorAccCode.Should().Be("02");
        result.SubMinorAccDesc.Should().Be("Unit No. II");
        result.MinorAccCode.Should().Be("202");
        result.MinorAccDesc.Should().Be("Ganguwal and Kotla Power Houses");
    }
}
