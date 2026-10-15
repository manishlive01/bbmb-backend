using BBMB.BudgetService.Features.BudgetProcess.Recommendations.GetRows.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
namespace BBMB.BudgetService.Features.BudgetProcess.Recommendations.GetRows
{
    public sealed class GetRecommendationRowsEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budget-cycles/{cycleId:long}/recommendations",
                    async (long cycleId, IMediator mediator, CancellationToken ct, [FromQuery] string category = "DNA") =>
                        BudgetResults.Ok(await mediator.Send(new GetRecommendationRowsQuery(cycleId, category), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetRecommendationRows")
                .WithTags("BudgetRecommendations")
                .Produces<Response<List<RecommendationRowDto>>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound);
        }
    }
}
