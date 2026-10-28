using BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetFigureHistory.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.ReadRequests;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetFigureHistory
{
    public sealed class GetFigureHistoryEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // Read via POST: proposalId and lineId travel in the body, not the URL (security requirement).
            app.MapPost("/api/budget-proposals/demand-lines/figure-history",
                    async (FigureHistoryRequestDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new GetFigureHistoryQuery(dto.ProposalId, dto.LineId), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetFigureHistory")
                .WithTags("BudgetDemandLines")
                .Produces<Response<List<FigureHistoryItemDto>>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
