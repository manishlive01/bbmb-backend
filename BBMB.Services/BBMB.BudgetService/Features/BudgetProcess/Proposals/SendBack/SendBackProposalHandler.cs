using BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.SendBack
{
    /// <summary>Design §8.3 SendBack, §6.2 T19: Finance sends the proposal back; the remark is required by T-16 (M-12). Right F10.Act (WORKS) / F13.Act (head-wise), authorised by the service.</summary>
    public sealed class SendBackProposalHandler(IProposalTransitionService transitions) : IRequestHandler<SendBackProposalCommand, Response>
    {
        public Task<Response> Handle(SendBackProposalCommand request, CancellationToken ct) =>
            transitions.MoveAsync(request.ProposalId, ProposalAction.SendBack, request.Remark, ct);
    }
}
