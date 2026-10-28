using BBMB.BudgetService.Features.BudgetProcess.Allocations.Preview.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.ReadRequests;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Allocations.Preview
{
    public sealed class PreviewAllocationEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // Read via POST: cycleId and the approving body travel in the body, not the URL (security requirement).
            app.MapPost("/api/budget-cycles/allocations/preview",
                    async (AllocationPreviewRequestDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new PreviewAllocationQuery(dto.CycleId, dto.Body), ct)))
                .RequireAuthorization("BudgetRight:F19.Allocate")
                .WithName("PreviewAllocation")
                .WithTags("Allocations")
                .Produces<Response<AllocationPreviewDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response<AllocationPreviewDto>>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
