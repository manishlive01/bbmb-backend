using BBMB.BudgetService.Features.BudgetProcess.Allocations.Preview.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Allocations.Preview
{
    public sealed class PreviewAllocationEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budget-cycles/{cycleId:long}/allocations/preview",
                    async (long cycleId, string body, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new PreviewAllocationQuery(cycleId, body), ct)))
                .RequireAuthorization("BudgetRight:F19.Allocate")
                .WithName("PreviewAllocation")
                .WithTags("Allocations")
                .Produces<Response<AllocationPreviewDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response<AllocationPreviewDto>>(StatusCodes.Status409Conflict);
        }
    }
}
