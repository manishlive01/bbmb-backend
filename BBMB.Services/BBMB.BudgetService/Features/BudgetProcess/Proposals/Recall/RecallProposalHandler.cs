using BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Recall
{
    /// <summary>
    /// Design §8.3 Recall, §6.2 T04, T04b, T09, T09b, T09c, T13, T13b, T13c, T17: the office that made the last
    /// move takes the proposal back to the <c>from_state</c> of that movement, before the office above has opened
    /// it (BUD-214, BUD-215). Who may recall and the target are resolved by <see cref="TransitionResolver"/>;
    /// anything else → M-19. Audited as action RECALL (BUD-217). Right Proposal.Recall (endpoint and service).
    /// </summary>
    public sealed class RecallProposalHandler(IProposalTransitionService transitions)
        : IRequestHandler<RecallProposalCommand, Response>
    {
        public Task<Response> Handle(RecallProposalCommand request, CancellationToken ct) =>
            transitions.MoveAsync(request.ProposalId, ProposalAction.Recall, null, ct);
    }
}
