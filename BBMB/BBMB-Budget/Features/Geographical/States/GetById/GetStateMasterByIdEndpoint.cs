using BBMB_Budget.Features.Geographical.States.GetById.Dtos;
using BBMB_Budget.Shared.Endpoints;
using BBMB_Budget.Shared.RequestDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB_Budget.Features.Geographical.States.GetById
{
    public class GetStateMasterByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/states/Get-State-ById", async (
                [FromBody] GetByIdRequestDto dto,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new GetStateMasterByIdCommand(dto), ct);

                return result == null
                    ? Results.NotFound("State not found")
                    : Results.Ok(result);
            })
            .Produces<GetStateMasterByIdResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Get State By Id")
            .WithDescription("Returns state details using a universal GetById request DTO")
            .WithTags("StateMaster");
        }
    }
}
