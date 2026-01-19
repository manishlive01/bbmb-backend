using BBMB.BudgetService.Features.Estimates.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Estimates.GetByWork
{
    public sealed record GetEstimatesByWorkQuery(
        string WorkCode,
        int PageNumber,
        int PageSize
    ) : IRequest<PagedResponse<GetEstimateListResponseDto>>;
}
