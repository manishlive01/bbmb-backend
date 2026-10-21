using BBMB.BudgetService.Features.BudgetProcess.Proposals.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Open
{
    public sealed class OpenProposalEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // Two right keys: Budget.View here, the key in ProposalTransitionService (design §8.1).
            app.MapPost("/api/budget-proposals/{id:long}/open", async (long id, IMediator mediator, CancellationToken ct) =>
                    BudgetResults.Ok(await mediator.Send(new OpenProposalCommand(id), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("OpenProposal")
                .WithTags("BudgetProposals")
                .Produces<Response<ProposalStateResponseDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status409Conflict);
        }
    }
}
