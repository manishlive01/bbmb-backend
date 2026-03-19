using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.Divisions.GetById
{
    public class GetDivisionByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/divisions/{code:int}", async (int code, IMediator mediator) =>
            {
                try
                {
                    var query = new GetDivisionByIdQuery(code);
                    var result = await mediator.Send(query);
                    return Results.Ok(new
                    {
                        message = "Division retrieved successfully",
                        data = result
                    });
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new
                    {
                        error = "division_not_found",
                        message = ex.Message,
                        timestamp = DateTime.UtcNow
                    });
                }
            })
            .WithName("GetDivisionById")
            .WithTags("Divisions")
            .WithSummary("Get a division by code")
            .WithDescription("Retrieves a specific division by its code")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
