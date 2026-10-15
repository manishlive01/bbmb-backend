using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Reopen
{
    public sealed record ReopenProposalCommand(long ProposalId, string? Remark) : IRequest<Response>;
}
