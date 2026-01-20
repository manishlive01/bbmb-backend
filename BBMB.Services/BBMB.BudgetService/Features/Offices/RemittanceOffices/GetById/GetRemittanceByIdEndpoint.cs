using BBMB.BudgetService.Features.Pools.GetById;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Offices.RemittanceOffices.GetById
{
    public class GetRemittanceByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/remittance/{code}", async (string code, IMediator mediator) =>
            {
                try
                {
                    var query = new GetRemittanceByIdQuery(code);
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
            .WithName("GetRemittanceById")
             .WithTags("OfficeRemitance")
            .WithSummary("Get a remittance by code")
            .WithDescription("Retrieves a specific remittance by its code")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
