using BBMB.BudgetService.Features.Estimates.Create;
using BBMB.BudgetService.Features.Estimates.Create.Dtos;
using BBMB.BudgetService.Features.Estimates.Update;
using BBMB.BudgetService.Features.Estimates.Update.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BBMB.BudgetService.Tests.Features.Estimates;

/// <summary>Design §3 D-3 and §4.5 step 4.</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class EstimateValidatorTests(BudgetDatabaseFixture fixture)
{
    private ServiceProvider Services()
    {
        var services = new ServiceCollection();
        services.AddDbContext<BudgetContext>(o => o.UseNpgsql(fixture.PlainConnectionString));
        // Same registration as Program.cs.
        services.AddValidatorsFromAssembly(typeof(BudgetContext).Assembly);
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task D3_CreateEstimateValidator_is_resolved_for_the_command_and_refuses_paise()
    {
        await using var sp = Services();
        await using var scope = sp.CreateAsyncScope();
        var validator = scope.ServiceProvider.GetService<IValidator<CreateEstimateCommand>>();
        validator.Should().BeOfType<CreateEstimateValidator>("ValidationBehavior resolves IValidator<TCommand>");

        var result = await validator!.ValidateAsync(new CreateEstimateCommand(new CreateEstimateDto
        {
            WorkCode = 1,
            EstimateCode = "E1",
            EstimateAmt = 100.50m,
            FinYearEnd = 2026,
            DivisionCode = 1,
        }));
        result.Errors.Should().Contain(e => e.ErrorCode == "M-22" && e.PropertyName.EndsWith("EstimateAmt"));
    }

    [Fact]
    public async Task D3_UpdateEstimateValidator_is_resolved_for_the_command_and_refuses_paise()
    {
        await using var sp = Services();
        await using var scope = sp.CreateAsyncScope();
        var validator = scope.ServiceProvider.GetService<IValidator<UpdateEstimateCommand>>();
        validator.Should().BeOfType<UpdateEstimateValidator>();

        var result = await validator!.ValidateAsync(new UpdateEstimateCommand(1, "E1", new UpdateEstimateDto
        {
            EstimateAmt = 100.50m,
            FinYearEnd = 2026,
            DivisionCode = 1,
        }));
        result.Errors.Should().Contain(e => e.ErrorCode == "M-22");
    }

    [Fact]
    public async Task BUD013_whole_rupee_estimate_amount_raises_no_money_error()
    {
        await using var sp = Services();
        await using var scope = sp.CreateAsyncScope();
        var validator = scope.ServiceProvider.GetRequiredService<IValidator<CreateEstimateCommand>>();
        var result = await validator.ValidateAsync(new CreateEstimateCommand(new CreateEstimateDto
        {
            WorkCode = 1,
            EstimateCode = "E1",
            EstimateAmt = 100m,
            FinYearEnd = 2026,
            DivisionCode = 1,
        }));
        result.Errors.Should().NotContain(e => e.ErrorCode == "M-22");
    }
}
