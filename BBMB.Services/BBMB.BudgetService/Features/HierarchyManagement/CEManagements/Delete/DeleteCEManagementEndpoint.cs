using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.HierarchyManagement.CEManagements.Delete
{
    public class DeleteCEManagementEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/ce-management/{ceId:int}",   // ✅ add type constraint
                async (
                    [FromRoute] int ceId,                  // ✅ explicit binding
                    IMediator mediator) =>
                {
                    if (ceId <= 0)
                    {
                        return Results.BadRequest(new Response
                        {
                            IsSucced = false,
                            Message = "CE Code is required"
                        });
                    }

                    var command = new DeleteCEManagementCommand(ceId);
                    var response = await mediator.Send(command);

                    if (!response.IsSucced)
                        return Results.NotFound(response);

                    return Results.Ok(response);
                })
                .WithName("DeleteCEManagement")
                .WithTags("CEManagement")
                .WithSummary("Delete CE Management record")
                .WithDescription("Deletes a CE Management record by CE Code")
                .Produces<Response>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status400BadRequest);
        }
    }
}