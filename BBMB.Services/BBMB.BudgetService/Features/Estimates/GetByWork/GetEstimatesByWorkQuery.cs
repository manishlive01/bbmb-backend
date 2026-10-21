using BBMB.BudgetService.Features.Estimates.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.Estimates.GetByWork
{
    // P4 (design §4.6): estimates of a work are addressed by the work's stable id, not the old int code.
    public sealed record GetEstimatesByWorkQuery(
        int WorkId,
        int PageNumber,
        int PageSize
    ) : IRequest<PagedResponse<GetEstimateListResponseDto>>;
}
