using BBMB.BudgetService.Features.BudgetProcess.DemandLines.KeyExpenditure.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.KeyExpenditure
{
    public sealed class KeyExpenditureEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // F07.Enter / F11.Enter by category, authorised in the handler (design §8.1).
            app.MapPost("/api/budget-proposals/{id:long}/demand-lines/{lineId:long}/expenditure",
                    async (long id, long lineId, KeyExpenditureDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(await mediator.Send(new KeyExpenditureCommand(id, lineId, dto), ct),
                            r => Results.Created($"/api/budget-proposals/{id}/demand-lines/{lineId}/expenditure", (object)r)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("KeyExpenditure")
                .WithTags("BudgetDemandLines")
                .Produces<Response<long>>(StatusCodes.Status201Created)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
