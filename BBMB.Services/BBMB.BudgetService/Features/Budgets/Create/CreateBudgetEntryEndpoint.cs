using BBMB.BudgetService.Features.Budgets.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Budgets.Create
{
    public class CreateBudgetEntryEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/budgets", async (CreateBudgetEntryDto dto, IMediator mediator) =>
            {
                var command = new CreateBudgetEntryCommand(dto);
                var response = await mediator.Send(command);

                if (!response.IsSucced)
                {
                    return Results.Conflict(response);
                }

                return Results.Created($"/api/budgets", response);
            })
            .RequireAuthorization("BudgetRight:Legacy.BudgetEntry")
            .WithName("CreateBudgetEntry")
            .WithTags("Budgets")
            .WithSummary("Create a new budget entry")
            .WithDescription("Creates a new budget entry with auto-generated ID")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict);
        }
    }
}
