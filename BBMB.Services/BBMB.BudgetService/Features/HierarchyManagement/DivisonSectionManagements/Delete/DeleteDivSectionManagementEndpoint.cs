using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.Delete
{
    public class DeleteDivSectionManagementEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/div-section-management/{divSectionId:int}",
                async ([FromRoute] int divSectionId, IMediator mediator) =>
                {
                    if (divSectionId <= 0)
                    {
                        return Results.BadRequest(new Response
                        {
                            IsSucced = false,
                            Message = "DivSectionId is required"
                        });
                    }

                    var command = new DeleteDivSectionManagementCommand(divSectionId);
                    var response = await mediator.Send(command);

                    if (!response.IsSucced)
                        return Results.NotFound(response);

                    return Results.Ok(response);
                })
                .WithName("DeleteDivSectionManagement")
                .WithTags("DivSectionManagement")
                .WithSummary("Delete Division Section")
                .WithDescription("Deletes a Division Section record by Id")
                .Produces<Response>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status400BadRequest);
        }
    }
}