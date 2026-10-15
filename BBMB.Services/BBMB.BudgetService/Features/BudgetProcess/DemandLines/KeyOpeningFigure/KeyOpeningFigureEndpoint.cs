using BBMB.BudgetService.Features.BudgetProcess.DemandLines.KeyOpeningFigure.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.KeyOpeningFigure
{
    public sealed class KeyOpeningFigureEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // F07.Enter / F11.Enter by category, authorised in the handler (design §8.1).
            app.MapPost("/api/budget-proposals/{id:long}/demand-lines/{lineId:long}/opening-figures",
                    async (long id, long lineId, KeyOpeningFigureDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new KeyOpeningFigureCommand(id, lineId, dto), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("KeyOpeningFigure")
                .WithTags("BudgetDemandLines")
                .Produces<Response<long>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
