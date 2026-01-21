using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Pools.GetById
{
    public class GetRemittanceByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/pools/{code}", async (string code, IMediator mediator) =>
            {
                try
                {
                    var query = new GetPoolByIdQuery(code);
                    var result = await mediator.Send(query);
                    return Results.Ok(new
                    {
                        message = "Pool retrieved successfully",
                        data = result
                    });
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new
                    {
                        error = "pool_not_found",
                        message = ex.Message,
                        timestamp = DateTime.UtcNow
                    });
                }
            })
            .WithName("GetPoolById")
            .WithTags("Pools")
            .WithSummary("Get a pool by code")
            .WithDescription("Retrieves a specific pool by its code")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
