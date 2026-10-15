using BBMB.BudgetService.Features.BudgetProcess.DemandLines.AlterFigures.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.AlterFigures
{
    public sealed class AlterFiguresEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // Review.Examine / F10.Act by state: Budget.View here, the key in the handler (design §8.1).
            app.MapPut("/api/budget-proposals/{id:long}/demand-lines/{lineId:long}/figures",
                    async (long id, long lineId, AlterFiguresDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new AlterFiguresCommand(id, lineId, dto), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("AlterFigures")
                .WithTags("BudgetDemandLines")
                .Produces<Response<SaveDemandLineResponseDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response<RuleFailuresDto>>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
