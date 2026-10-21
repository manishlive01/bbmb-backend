using BBMB.BudgetService.Features.BudgetProcess.Reports.GetConsolidation.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Reports.GetConsolidation
{
    public sealed class GetConsolidationEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budget-cycles/{cycleId:long}/consolidation",
                    async (long cycleId, string? category, string? wing, string? groupBy, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(await mediator.Send(new GetConsolidationQuery(cycleId, category, wing, groupBy), ct),
                            r => Results.Ok((object)r)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetConsolidation")
                .WithTags("BudgetReports")
                .Produces<Response<ConsolidationResultDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden);
        }
    }
}
