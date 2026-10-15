using BBMB.BudgetService.Features.BudgetProcess.Proposals.GetById.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.GetById
{
    public sealed record GetProposalByIdQuery(long ProposalId) : IRequest<Response<GetProposalByIdResponseDto>>;
}
