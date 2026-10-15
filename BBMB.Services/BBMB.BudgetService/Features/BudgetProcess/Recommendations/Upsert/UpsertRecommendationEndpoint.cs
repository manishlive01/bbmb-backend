using BBMB.BudgetService.Features.BudgetProcess.Recommendations.Upsert.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Recommendations.Upsert
{
    public sealed class UpsertRecommendationEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/budget-cycles/{cycleId:long}/recommendations",
                    async (long cycleId, UpsertRecommendationDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new UpsertRecommendationCommand(cycleId, dto), ct)))
                .RequireAuthorization("BudgetRight:F13.Act")
                .WithName("UpsertRecommendation")
                .WithTags("BudgetRecommendations")
                .Produces<Response<long>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
