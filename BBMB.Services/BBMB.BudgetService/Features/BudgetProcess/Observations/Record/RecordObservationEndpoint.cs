using BBMB.BudgetService.Features.BudgetProcess.Observations.Record.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Observations.Record
{
    public sealed class RecordObservationEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // Review.Examine / F10.Act / F13.Act by state and category: Budget.View here, the key in the handler (§8.1).
            app.MapPost("/api/budget-proposals/{id:long}/observations",
                    async (long id, RecordObservationDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(await mediator.Send(new RecordObservationCommand(id, dto), ct),
                            r => Results.Created($"/api/budget-proposals/{id}/observations", r)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("RecordObservation")
                .WithTags("BudgetObservations")
                .Produces<Response<long>>(StatusCodes.Status201Created)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
