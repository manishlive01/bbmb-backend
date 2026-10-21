using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.SendBack
{
    public sealed record SendBackProposalCommand(long ProposalId, string? Remark) : IRequest<Response>;
}
