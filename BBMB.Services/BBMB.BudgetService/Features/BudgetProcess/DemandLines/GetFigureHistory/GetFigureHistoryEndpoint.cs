using BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetFigureHistory.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetFigureHistory
{
    public sealed class GetFigureHistoryEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budget-proposals/{id:long}/demand-lines/{lineId:long}/figure-history",
                    async (long id, long lineId, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new GetFigureHistoryQuery(id, lineId), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetFigureHistory")
                .WithTags("BudgetDemandLines")
                .Produces<Response<List<FigureHistoryItemDto>>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound);
        }
    }
}
