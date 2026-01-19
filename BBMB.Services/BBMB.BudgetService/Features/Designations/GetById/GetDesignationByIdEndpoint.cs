using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Designations.GetById
{
    public class GetDesignationByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/designations/{code}", async (string code, IMediator mediator) =>
            {
                try
                {
                    var query = new GetDesignationByIdQuery(code);
                    var result = await mediator.Send(query);
                    return Results.Ok(new
                    {
                        message = "Designation retrieved successfully",
                        data = result
                    });
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new
                    {
                        error = "designation_not_found",
                        message = ex.Message,
                        timestamp = DateTime.UtcNow
                    });
                }
            })
            .WithName("GetDesignationById")
            .WithTags("Designations")
            .WithSummary("Get a designation by code")
            .WithDescription("Retrieves a specific designation by its code")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
