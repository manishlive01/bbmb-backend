using BBMB.BudgetService.Features.BudgetProcess.Observations.GetByProposal.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Observations.GetByProposal
{
    public sealed class GetObservationsByProposalEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budget-proposals/{id:long}/observations", async (long id, IMediator mediator, CancellationToken ct) =>
                    BudgetResults.Ok(await mediator.Send(new GetObservationsByProposalQuery(id), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetObservationsByProposal")
                .WithTags("BudgetObservations")
                .Produces<Response<List<ObservationResponseDto>>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound);
        }
    }
}
