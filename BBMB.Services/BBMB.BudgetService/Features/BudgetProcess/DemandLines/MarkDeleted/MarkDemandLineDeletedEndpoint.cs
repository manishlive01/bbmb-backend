using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.MarkDeleted
{
    public sealed class MarkDemandLineDeletedEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // F07.Enter / F11.Enter by category, authorised in the handler (design §8.1).
            app.MapDelete("/api/budget-proposals/{id:long}/demand-lines/{lineId:long}",
                    async (long id, long lineId, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(await mediator.Send(new MarkDemandLineDeletedCommand(id, lineId), ct),
                            _ => Results.NoContent()))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("MarkDemandLineDeleted")
                .WithTags("BudgetDemandLines")
                .Produces(StatusCodes.Status204NoContent)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status409Conflict);
        }
    }
}
