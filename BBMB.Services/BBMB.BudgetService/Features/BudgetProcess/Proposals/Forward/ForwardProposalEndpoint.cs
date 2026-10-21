using BBMB.BudgetService.Features.BudgetProcess.Proposals.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Forward
{
    public sealed class ForwardProposalEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/budget-proposals/{id:long}/forward", async (long id, TransitionRemarkDto? dto, IMediator mediator, CancellationToken ct) =>
                    BudgetResults.Ok(await mediator.Send(new ForwardProposalCommand(id, dto?.Remark), ct)))
                .RequireAuthorization("BudgetRight:Review.Examine")
                .WithName("ForwardProposal")
                .WithTags("BudgetProposals")
                .Produces<Response<ProposalStateResponseDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response<RuleFailuresDto>>(StatusCodes.Status409Conflict);
        }
    }
}
