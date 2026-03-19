using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRManagements.GetById
{
    public class GetCRManagementByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/cr-management/{crCode}", async (int crCode, IMediator mediator) =>
            {
                try
                {
                    var query = new GetCRManagementByIdQuery(crCode);
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
