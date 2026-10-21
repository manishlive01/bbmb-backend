using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.ForwardToFinance
{
    public sealed record ForwardToFinanceProposalCommand(long ProposalId, string? Remark) : IRequest<Response>;
}
