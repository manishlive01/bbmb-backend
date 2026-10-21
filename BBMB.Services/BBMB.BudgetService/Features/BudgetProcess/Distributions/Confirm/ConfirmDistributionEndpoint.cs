using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Distributions.Confirm
{
    public sealed class ConfirmDistributionEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/budget-cycles/{cycleId:long}/distributions/{recommendationId:long}/confirm",
                    async (long cycleId, long recommendationId, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(await mediator.Send(new ConfirmDistributionCommand(cycleId, recommendationId), ct),
                            r => Results.Ok((object)r)))
                .RequireAuthorization("BudgetRight:F14.Distribute")
                .WithName("ConfirmDistribution")
                .WithTags("BudgetDistributions")
                .Produces<Response>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status409Conflict);
        }
    }
}
