using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.HierarchyManagement.Divisions.Delete
{
    public class DeleteDivisionEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/divisions/{divId:int}",
                async ([FromRoute] int divId, IMediator mediator) =>
                {
                    if (divId <= 0)
                    {
                        return Results.BadRequest(new
                        {
                            IsSucced = false,
                            Message = "Division code is required"
                        });
                    }

                    var command = new DeleteDivisionCommand(divId);
                    var response = await mediator.Send(command);

                    if (!response.IsSucced)
                    {
                        // ✅ safer null check
                        if (!string.IsNullOrEmpty(response.Message) &&
                            response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                            return Results.NotFound(response);

                        return Results.Conflict(response);
                    }

                    return Results.Ok(response);
                })
            .WithName("DeleteDivision")
            .WithTags("Divisions")
            .WithSummary("Delete a division")
            .WithDescription("Deletes a division")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status400BadRequest); // ✅ added
        }
    }
}