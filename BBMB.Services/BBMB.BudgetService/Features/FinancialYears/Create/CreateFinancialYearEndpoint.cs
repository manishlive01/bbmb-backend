using BBMB.BudgetService.Features.FinancialYears.Create.Dtos;
using BBMB.BudgetService.Features.Geographical.States.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.FinancialYears.Create
{
    public class CreateFinancialYearEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/Create-FinancialYear", async (
                CreateFinancialYearDto dto,
                IMediator mediator) =>
            {
                var command = new CreateFinancialYearCommand(dto);

                var response = await mediator.Send(command);
                if (response.IsSucced == false)
                {
                    return Results.BadRequest(response);
                }
                return Results.Ok(response);
            })
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Create Financial Year")
            .WithDescription("Creates a new Financial Year entry")
            .WithName("Create Financial Year")
            .WithTags("FinancialYear");
        }
    }
}
