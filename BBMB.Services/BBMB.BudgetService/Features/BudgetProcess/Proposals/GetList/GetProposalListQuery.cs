using BBMB.BudgetService.Features.BudgetProcess.Proposals.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.GetList
{
    public sealed record GetProposalListQuery(
        int PageNumber, int PageSize, long? CycleId, string? Category, string? State, long? OfficeId, bool PendingWithMe)
        : IRequest<PagedResponse<GetProposalListResponseDto>>;
}
