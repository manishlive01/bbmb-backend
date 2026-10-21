using BBMB.BudgetService.Features.BudgetProcess.Proposals.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.ReleaseLease
{
    public sealed record ReleaseLeaseCommand(long ProposalId) : IRequest<Response<LeaseResponseDto>>;
}
