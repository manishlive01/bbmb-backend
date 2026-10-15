using BBMB.BudgetService.Features.BudgetProcess.Distributions.Save.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Distributions.Save
{
    public sealed class SaveDistributionEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/budget-cycles/{cycleId:long}/distributions",
                    async (long cycleId, SaveDistributionDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(await mediator.Send(new SaveDistributionCommand(cycleId, dto), ct),
                            r => Results.Ok((object)r)))
                .RequireAuthorization("BudgetRight:F14.Distribute")
                .WithName("SaveDistribution")
                .WithTags("BudgetDistributions")
                .Produces<Response>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
