using BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Open
{
    /// <summary>Design §8.3 Open, §6.2 T05, T10, T14, T18: the holder office opens what was sent to it; the office below can no longer recall it (BUD-215). Right Review.Examine, or Finance.Open at S-09, authorised by the service (two keys: Budget.View at the endpoint, §8.1).</summary>
    public sealed class OpenProposalHandler(IProposalTransitionService transitions) : IRequestHandler<OpenProposalCommand, Response>
    {
        public Task<Response> Handle(OpenProposalCommand request, CancellationToken ct) =>
            transitions.MoveAsync(request.ProposalId, ProposalAction.Open, null, ct);
    }
}
