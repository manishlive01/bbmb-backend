using BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.ForwardToFinance
{
    /// <summary>Design §8.3 ForwardToFinance, §6.2 T16: the Secretariat sends the proposal to Finance (BUD-118). Only the Secretariat holds the proposal at S-08, and only SECRETARY holds Review.ForwardFinance in the rights matrix.</summary>
    public sealed class ForwardToFinanceProposalHandler(IProposalTransitionService transitions) : IRequestHandler<ForwardToFinanceProposalCommand, Response>
    {
        public Task<Response> Handle(ForwardToFinanceProposalCommand request, CancellationToken ct) =>
            transitions.MoveAsync(request.ProposalId, ProposalAction.ForwardFinance, request.Remark, ct);
    }
}
