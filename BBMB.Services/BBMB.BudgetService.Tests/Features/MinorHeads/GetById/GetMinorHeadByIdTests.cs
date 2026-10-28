using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetById;
using BBMB.BudgetService.Tests.Common;
using FluentAssertions;

namespace BBMB.BudgetService.Tests.Features.MinorHeads.GetById;

public class GetMinorHeadByIdTests : InMemoryTestBase
{
    private static readonly DateTime ModifiedOn = new(2026, 1, 15, 9, 30, 0, DateTimeKind.Utc);

    [Fact]
     public async Task Should_Return_MinorHead_When_Code_Exists()
    {
        // Arrange
        Context.MinorHeads.Add(new MinorHead
        {
            MinorAccCode = "201",
            MinorAccDesc = "Bhakra Power Houses - Power Wing",
            ModificationDate = ModifiedOn,
            UserCode = "SYSTEM"
        });
        await Context.SaveChangesAsync();

        var handler = new GetMinorHeadByIdHandler(Context);

        // Act
        var result = await handler.Handle(new GetMinorHeadByIdQuery("201"), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.MinorAccCode.Should().Be("201");
        result.MinorAccDesc.Should().Be("Bhakra Power Houses - Power Wing");
        result.ModificationDate.Should().Be(ModifiedOn);
        result.UserCode.Should().Be("SYSTEM");
    }

    [Fact]
    public async Task Should_Throw_KeyNotFound_When_Code_Does_Not_Exist()
    {
        // Arrange
        var handler = new GetMinorHeadByIdHandler(Context);

        // Act
        Func<Task> act = () => handler.Handle(new GetMinorHeadByIdQuery("999"), CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("*999*");
    }

    [Fact]
    public async Task Should_Return_Only_The_Requested_MinorHead_When_Multiple_Exist()
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
            },
            new MinorHead
            {
                MinorAccCode = "203",
                MinorAccDesc = "Dehar Power House",
                ModificationDate = ModifiedOn,
                UserCode = "SYSTEM"
            });
        await Context.SaveChangesAsync();

        var handler = new GetMinorHeadByIdHandler(Context);

        // Act
        var result = await handler.Handle(new GetMinorHeadByIdQuery("202"), CancellationToken.None);

        // Assert
        result.MinorAccCode.Should().Be("202");
        result.MinorAccDesc.Should().Be("Ganguwal and Kotla Power Houses");
    }
}
