using BBMB.BudgetService.Features.BudgetProcess.Proposals.PlaceBeforeBsc.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.PlaceBeforeBsc
{
    public sealed class PlaceBeforeBscEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/budget-cycles/{cycleId:long}/place-before-bsc",
                    async (long cycleId, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new PlaceBeforeBscCommand(cycleId), ct)))
                .RequireAuthorization("BudgetRight:F19.Allocate")
                .WithName("PlaceBeforeBsc")
                .WithTags("BudgetProposals")
                .Produces<Response<BulkTransitionResponseDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response<BulkTransitionResponseDto>>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
