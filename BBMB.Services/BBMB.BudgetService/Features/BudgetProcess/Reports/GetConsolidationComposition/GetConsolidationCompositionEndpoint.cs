using BBMB.BudgetService.Features.BudgetProcess.Reports.GetConsolidationComposition.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Reports.GetConsolidationComposition
{
    public sealed class GetConsolidationCompositionEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budget-cycles/{cycleId:long}/consolidation/composition",
                    async (long cycleId, string key, string? category, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(await mediator.Send(new GetConsolidationCompositionQuery(cycleId, key, category), ct),
                            r => Results.Ok((object)r)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetConsolidationComposition")
                .WithTags("BudgetReports")
                .Produces<Response<List<CompositionLineDto>>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden);
        }
    }
}
