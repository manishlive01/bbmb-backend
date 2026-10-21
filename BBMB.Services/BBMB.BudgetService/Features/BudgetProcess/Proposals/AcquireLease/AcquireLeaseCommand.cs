using BBMB.BudgetService.Features.BudgetProcess.Proposals.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.AcquireLease
{
    public sealed record AcquireLeaseCommand(long ProposalId) : IRequest<Response<LeaseResponseDto>>;
}
