using BBMB.BudgetService.Features.BudgetProcess.Freezes.Freeze.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Freezes.Freeze
{
    public sealed class FreezeEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/budget-cycles/{cycleId:long}/freeze",
                    async (long cycleId, FreezeRequestDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new FreezeCommand(cycleId, dto), ct)))
                .RequireAuthorization("BudgetRight:F20.Freeze")
                .WithName("FreezeCycle")
                .WithTags("Freezes")
                .Produces<Response>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response<FreezeRefusalDto>>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
