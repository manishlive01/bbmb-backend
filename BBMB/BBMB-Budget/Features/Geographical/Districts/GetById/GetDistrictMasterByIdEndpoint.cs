using BBMB_Budget.Features.Geographical.Districts.GetById.Dtos;
using BBMB_Budget.Shared.Endpoints;
using BBMB_Budget.Shared.RequestDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB_Budget.Features.Geographical.Districts.GetById
{
    public class GetDistrictMasterByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/Get-District-ById", async (
                [FromBody] GetByIdRequestDto dto,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new GetDistrictMasterByIdCommand(dto), ct);

                return result == null
                    ? Results.NotFound("District not found")
                    : Results.Ok(result);
            })
            .Produces<GetDistrictMasterByIdResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Get District By Id")
            .WithDescription("Returns District details using a universal GetById request DTO")
            .WithTags("DistrictMaster");
        }
    }
}
