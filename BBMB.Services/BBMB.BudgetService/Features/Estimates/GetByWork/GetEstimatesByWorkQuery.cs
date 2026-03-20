using BBMB.BudgetService.Features.Estimates.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Estimates.GetByWork
{
    public sealed record GetEstimatesByWorkQuery(
        int WorkCode,
        int PageNumber,
        int PageSize
    ) : IRequest<PagedResponse<GetEstimateListResponseDto>>;
}
