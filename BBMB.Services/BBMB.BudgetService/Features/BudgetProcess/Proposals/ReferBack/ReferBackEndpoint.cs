using BBMB.BudgetService.Features.BudgetProcess.Proposals.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.ReferBack
{
    public sealed class ReferBackEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // Two right keys: Budget.View here, F19.Allocate in ProposalTransitionService (design §8.1).
            app.MapPost("/api/budget-proposals/{id:long}/refer-back",
                    async (long id, TransitionRemarkDto? dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new ReferBackCommand(id, dto?.Remark), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("ReferBackProposal")
                .WithTags("BudgetProposals")
                .Produces<Response<ProposalStateResponseDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status409Conflict);
        }
    }
}
