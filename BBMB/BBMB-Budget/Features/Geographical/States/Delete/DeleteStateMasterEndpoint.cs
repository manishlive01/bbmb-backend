using BBMB_Budget.Shared.Endpoints;
using BBMB_Budget.Shared.RequestDTOs;
using BBMB_Budget.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB_Budget.Features.Geographical.States.Delete
{
    public class DeleteStateMasterEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/states/Delete-State", async (
                [FromBody] GetByIdRequestDto dto,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var response = await mediator.Send(
                    new DeleteStateMasterCommand(dto), ct);

                return response.IsSucced
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            })
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces<Response>(StatusCodes.Status400BadRequest)
            .WithSummary("Delete State")
            .WithDescription("Soft deletes a state using StateMasterId")
            .WithTags("StateMaster");
        }
    }
}
