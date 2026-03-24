using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Delete
{
    public class DeleteCRSectionManagementEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/cr-section-management/{crSectionId:int}",
                async (
                    [FromRoute] int crSectionId,
                    IMediator mediator) =>
                {
                    if (crSectionId <= 0)
                    {
                        return Results.BadRequest(new Response
                        {
                            IsSucced = false,
                            Message = "CR Section Id is required"
                        });
                    }

                    var command = new DeleteCRSectionManagementCommand(crSectionId);
                    var response = await mediator.Send(command);

                    if (!response.IsSucced)
                        return Results.NotFound(response);

                    return Results.Ok(response);
                })
                .WithName("DeleteCRSectionManagement")
                .WithTags("CRSectionManagement")
                .Produces<Response>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status400BadRequest);
        }
    }
}