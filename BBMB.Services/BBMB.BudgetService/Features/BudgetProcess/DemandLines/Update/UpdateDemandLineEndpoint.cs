using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.Update
{
    public sealed class UpdateDemandLineEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // F07.Enter / F11.Enter by category, authorised in the handler (design §8.1).
            app.MapPut("/api/budget-proposals/{id:long}/demand-lines/{lineId:long}",
                    async (long id, long lineId, UpdateDemandLineDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new UpdateDemandLineCommand(id, lineId, dto), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("UpdateDemandLine")
                .WithTags("BudgetDemandLines")
                .Produces<Response<SaveDemandLineResponseDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response<RuleFailuresDto>>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
