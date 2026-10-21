using BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Reopen
{
    /// <summary>Design §8.3 Reopen, §6.2 T21 (OI-21): S-11 back to S-10 with a remark (M-12). Right F10.Act / F13.Act by category, authorised by the service.</summary>
    public sealed class ReopenProposalHandler(IProposalTransitionService transitions) : IRequestHandler<ReopenProposalCommand, Response>
    {
        public Task<Response> Handle(ReopenProposalCommand request, CancellationToken ct) =>
            transitions.MoveAsync(request.ProposalId, ProposalAction.Reopen, request.Remark, ct);
    }
}
