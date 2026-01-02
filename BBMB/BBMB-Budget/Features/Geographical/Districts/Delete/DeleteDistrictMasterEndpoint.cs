using BBMB_Budget.Shared.Endpoints;
using BBMB_Budget.Shared.RequestDTOs;
using BBMB_Budget.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB_Budget.Features.Geographical.Districts.Delete
{
    public class DeleteDistrictMasterEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/Districts/Delete-District", async (
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
