using BBMB.BudgetService.Features.Geographical.States.Create;
using BBMB.BudgetService.Features.Geographical.States.Create.Dtos;
using BBMB.BudgetService.Features.Geographical.States.Entities;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Tests.Features.Geographical.States.Create;

public class CreateStateMasterHandlerTests : PostgresTestBase
{
    [Fact]
    public async Task Should_Create_State_When_Not_Exists()
    {
        // Arrange
        var handler = new CreateStateMasterHandler(DbContext);

        var command = new CreateStateMasterCommand(new CreateStateMasterDto
        {
            StateCode = "PB",
            StateName = "Punjab"
        });

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        response.IsSucced.Should().BeTrue();
        response.Message.Should().Be("State created successfully");

        var state = await DbContext.StateMasters
            .SingleAsync(x => x.StateCode == "PB");

        state.IsDeleted.Should().BeFalse();
        state.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    // ------------------------------------------------------

    [Fact]
    public async Task Should_Fail_When_StateCode_Already_Exists_And_Not_Deleted()
    {
        // Arrange
        DbContext.StateMasters.Add(new StateMaster
        {
            StateCode = "HR",
            StateName = "Haryana",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        });

        await DbContext.SaveChangesAsync();

        var handler = new CreateStateMasterHandler(DbContext);

        var command = new CreateStateMasterCommand(new CreateStateMasterDto
        {
            StateCode = "HR",
            StateName = "Haryana"
        });

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        response.IsSucced.Should().BeFalse();
        response.Message.Should().Be("State code already exists");

        var count = await DbContext.StateMasters.CountAsync(x => x.StateCode == "HR");
        count.Should().Be(1);
    }

    // ------------------------------------------------------

    [Fact]
    public async Task Should_Allow_Create_When_Same_StateCode_Exists_But_IsDeleted()
    {
        // Arrange
        DbContext.StateMasters.Add(new StateMaster
        {
            StateCode = "RJ",
            StateName = "Rajasthan (Old)",
            IsDeleted = true,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        });

        await DbContext.SaveChangesAsync();

        var handler = new CreateStateMasterHandler(DbContext);

        var command = new CreateStateMasterCommand(new CreateStateMasterDto
        {
            StateCode = "RJ",
            StateName = "Rajasthan"
        });

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        response.IsSucced.Should().BeTrue();

        var states = await DbContext.StateMasters
            .Where(x => x.StateCode == "RJ")
            .ToListAsync();

        states.Should().HaveCount(2);
        states.Count(x => x.IsDeleted == false).Should().Be(1);
    }

    // ------------------------------------------------------

    [Fact]
    public async Task Should_Set_System_Fields_Correctly()
    {
        // Arrange
        var handler = new CreateStateMasterHandler(DbContext);

        var command = new CreateStateMasterCommand(new CreateStateMasterDto
        {
            StateCode = "UP",
            StateName = "Uttar Pradesh"
        });

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var state = await DbContext.StateMasters
            .SingleAsync(x => x.StateCode == "UP");

        state.IsDeleted.Should().BeFalse();
        state.CreatedAt.Should().NotBe(default);
    }

    // ------------------------------------------------------

    [Fact]
    public async Task Should_Create_Multiple_Different_States()
    {
        // Arrange
        var handler = new CreateStateMasterHandler(DbContext);

        var states = new[]
        {
            new CreateStateMasterDto { StateCode = "DL", StateName = "Delhi" },
            new CreateStateMasterDto { StateCode = "MP", StateName = "Madhya Pradesh" }
        };

        // Act
        foreach (var dto in states)
        {
            await handler.Handle(
                new CreateStateMasterCommand(dto),
                CancellationToken.None);
        }

        // Assert
        var count = await DbContext.StateMasters.CountAsync();
        count.Should().Be(2);
    }

    // ------------------------------------------------------

    [Fact]
    public async Task Should_Treat_StateCode_As_Case_Sensitive_By_Default()
    {
        // Arrange
        var handler = new CreateStateMasterHandler(DbContext);

        await handler.Handle(
            new CreateStateMasterCommand(
                new CreateStateMasterDto
                {
                    StateCode = "pb",
                    StateName = "Punjab lower"
                }),
            CancellationToken.None);

        // Act
        var response = await handler.Handle(
            new CreateStateMasterCommand(
                new CreateStateMasterDto
                {
                    StateCode = "PB",
                    StateName = "Punjab upper"
                }),
            CancellationToken.None);

        // Assert
        response.IsSucced.Should().BeTrue();

        var count = await DbContext.StateMasters.CountAsync();
        count.Should().Be(2);
    }

    // ------------------------------------------------------

    [Fact]
    public async Task Should_Respect_Cancellation_Token()
    {
        // Arrange
        var handler = new CreateStateMasterHandler(DbContext);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var command = new CreateStateMasterCommand(new CreateStateMasterDto
        {
            StateCode = "JK",
            StateName = "Jammu & Kashmir"
        });

        // Act
        Func<Task> act = async () =>
            await handler.Handle(command, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
