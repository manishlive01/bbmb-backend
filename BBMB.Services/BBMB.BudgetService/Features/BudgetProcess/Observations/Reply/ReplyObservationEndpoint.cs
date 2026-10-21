using BBMB.BudgetService.Features.BudgetProcess.Observations.Reply.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Observations.Reply
{
    public sealed class ReplyObservationEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/budget-proposals/{id:long}/observations/{obsId:long}/reply",
                    async (long id, long obsId, ReplyObservationDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new ReplyObservationCommand(id, obsId, dto), ct)))
                .RequireAuthorization("BudgetRight:Proposal.Reply")
                .WithName("ReplyObservation")
                .WithTags("BudgetObservations")
                .Produces<Response>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
