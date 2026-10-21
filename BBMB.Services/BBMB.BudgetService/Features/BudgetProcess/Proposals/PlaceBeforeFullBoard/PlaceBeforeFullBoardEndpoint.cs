using BBMB.BudgetService.Features.BudgetProcess.Proposals.PlaceBeforeBsc.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.PlaceBeforeFullBoard
{
    public sealed class PlaceBeforeFullBoardEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/budget-cycles/{cycleId:long}/place-before-full-board",
                    async (long cycleId, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new PlaceBeforeFullBoardCommand(cycleId), ct)))
                .RequireAuthorization("BudgetRight:F19.Allocate")
                .WithName("PlaceBeforeFullBoard")
                .WithTags("BudgetProposals")
                .Produces<Response<BulkTransitionResponseDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response<BulkTransitionResponseDto>>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
