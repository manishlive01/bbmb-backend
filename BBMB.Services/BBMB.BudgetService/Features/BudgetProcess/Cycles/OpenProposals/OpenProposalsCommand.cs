using BBMB.BudgetService.Features.BudgetProcess.Cycles.OpenProposals.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Cycles.OpenProposals
{
    public sealed record OpenProposalsCommand(long CycleId, OpenProposalsDto Dto) : IRequest<Response<OpenProposalsResponseDto>>;
}
