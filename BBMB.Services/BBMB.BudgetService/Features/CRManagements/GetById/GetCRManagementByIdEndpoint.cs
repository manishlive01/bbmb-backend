using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.CRManagements.GetById
{
    public class GetCRManagementByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/cr-management/{id:int}", async (int id, IMediator mediator) =>
            {
                try
                {
                    var query = new GetCRManagementByIdQuery(id);
                    var response = await mediator.Send(query);
                    return Results.Ok(response);
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { message = ex.Message });
                }
            })
            .WithName("GetCRManagementById")
            .WithTags("CRManagement")
            .WithSummary("Get CR Management record by ID")
            .WithDescription("Retrieves CR Management details by primary key")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
