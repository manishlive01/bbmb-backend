using BBMB.BudgetService.Features.BudgetProcess.Observations.GetByProposal.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.ReadRequests;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Observations.GetByProposal
{
    public sealed class GetObservationsByProposalEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // Read via POST: proposalId travels in the body, not the URL (security requirement).
            app.MapPost("/api/budget-proposals/observations", async (ProposalReadRequestDto dto, IMediator mediator, CancellationToken ct) =>
                    BudgetResults.Ok(await mediator.Send(new GetObservationsByProposalQuery(dto.ProposalId), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetObservationsByProposal")
                .WithTags("BudgetObservations")
                .Produces<Response<List<ObservationResponseDto>>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
