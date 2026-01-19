using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Budgets.GetById
{
    public class GetBudgetEntryByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budgets/{id}", async (int id, IMediator mediator) =>
            {
                try
                {
                    var query = new GetBudgetEntryByIdQuery(id);
                    var response = await mediator.Send(query);
                    return Results.Ok(response);
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { message = ex.Message });
                }
            })
            .WithName("GetBudgetEntryById")
            .WithTags("Budgets")
            .WithSummary("Get a budget entry by ID")
            .WithDescription("Retrieves a budget entry with joined data (division description, work description, account code and description)")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
