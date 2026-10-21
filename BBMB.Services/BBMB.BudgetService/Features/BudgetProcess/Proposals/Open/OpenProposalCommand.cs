using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Open
{
    public sealed record OpenProposalCommand(long ProposalId) : IRequest<Response>;
}
