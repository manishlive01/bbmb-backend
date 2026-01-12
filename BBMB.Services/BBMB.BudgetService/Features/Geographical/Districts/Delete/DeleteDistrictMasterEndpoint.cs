using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.RequestDTOs;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Geographical.Districts.Delete
{
    public class DeleteDistrictMasterEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/Delete-District", async (
                [FromBody] GetByIdRequestDto dto,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var response = await mediator.Send(
                    new DeleteDistrictMasterCommand(dto), ct);

                return response.IsSucced
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            })
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces<Response>(StatusCodes.Status400BadRequest)
            .WithSummary("Delete District")
            .WithDescription("Soft deletes a District using DistrictMasterId")
            .WithTags("DistrictMaster");
        }
    }
}
