using BBMB.BudgetService.Features.BudgetProcess.Proposals.Return.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Return
{
    public sealed record ReturnProposalCommand(long ProposalId, ReturnProposalDto Dto) : IRequest<Response>;
}
