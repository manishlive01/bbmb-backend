using BBMB.BudgetService.Features.Budgets.GetById;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.CEManagements.GetById
{
    public class GetCEManagementByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/ce-management/{id:int}", async (int id, IMediator mediator) =>
            {
                try
                {
                    var query = new GetCEManagementByIdQuery(id);
                    var response = await mediator.Send(query);
                    return Results.Ok(response);
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { message = ex.Message });
                }
            })
             .WithName("GetCEManagementById")
             .WithTags("CEManagement")
             .WithSummary("Get CE Management record by ID")
             .WithDescription("Retrieves CE Management details by primary key")
             .Produces(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status404NotFound);


        }
    }
}
