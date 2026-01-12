using BBMB.BudgetService.Features.Geographical.States.GetById.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.RequestDTOs; 
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Geographical.States.GetById
{
    public class GetStateMasterByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/Get-State-ById", async (
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
