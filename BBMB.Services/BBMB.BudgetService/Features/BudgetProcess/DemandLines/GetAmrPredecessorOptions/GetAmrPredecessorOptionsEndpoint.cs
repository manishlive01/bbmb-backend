using BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetAmrPredecessorOptions.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetAmrPredecessorOptions
{
    public sealed class GetAmrPredecessorOptionsEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budget-proposals/{id:long}/amr-predecessor-options",
                    async (long id, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new GetAmrPredecessorOptionsQuery(id), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetAmrPredecessorOptions")
                .WithTags("BudgetDemandLines")
                .Produces<Response<List<AmrPredecessorOptionDto>>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound);
        }
    }
}
