using BBMB.BudgetService.Features.HierarchyManagement.CEManagements.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.HierarchyManagement.CEManagements.Update
{
    public class UpdateCEManagementEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/ce-management/{ceId:int}",
                    async (
                       [FromRoute] int ceId,
                      [FromBody] UpdateCEManagementDto dto,
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

                        var command = new UpdateCEManagementCommand(ceId, dto);
                        var response = await mediator.Send(command);

                        if (!response.IsSucced)
                            return Results.NotFound(response);

                        return Results.Ok(response);
                    })
                    .WithName("UpdateCEManagement")
                    .WithTags("CEManagement")
                    .WithSummary("Update CE Management record")
                    .WithDescription("Updates an existing CE Management record by CE Code")
                    .Produces<Response>(StatusCodes.Status200OK)
                    .Produces<Response>(StatusCodes.Status404NotFound)
                    .Produces<Response>(StatusCodes.Status400BadRequest);
        }
    }
}
