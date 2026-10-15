using BBMB.BudgetService.Features.BudgetProcess.Allocations.Allocate.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Allocations.Allocate
{
    public sealed class AllocateEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/budget-cycles/{cycleId:long}/allocations",
                    async (long cycleId, AllocateRequestDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new AllocateCommand(cycleId, dto), ct)))
                .RequireAuthorization("BudgetRight:F19.Allocate")
                .WithName("Allocate")
                .WithTags("Allocations")
                .Produces<Response<AllocationResponseDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response<AllocationPreviewMismatchDto>>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
