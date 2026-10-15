using BBMB.BudgetService.Features.BudgetProcess.EstimateAllocations.Save.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.EstimateAllocations.Save
{
    public sealed class SaveEstimateAllocationEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/provisions/{provisionId:long}/estimate-allocations",
                    async (long provisionId, SaveEstimateAllocationDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new SaveEstimateAllocationCommand(provisionId, dto), ct)))
                .RequireAuthorization("BudgetRight:F29.Allocate")
                .WithName("SaveEstimateAllocation")
                .WithTags("EstimateAllocations")
                .Produces<Response>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
