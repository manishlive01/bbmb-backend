using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Recall
{
    public sealed record RecallProposalCommand(long ProposalId) : IRequest<Response>;
}
