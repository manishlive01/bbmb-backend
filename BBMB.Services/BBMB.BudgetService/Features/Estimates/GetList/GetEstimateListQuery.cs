using BBMB.BudgetService.Features.Estimates.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Estimates.GetList
{
    public sealed record GetEstimateListQuery(
        int PageNumber,
        int PageSize,
        string? SortBy,
        bool SortDescending,
        string? WorkCode,
        int? DivisionCode,
        int? FinYearEnd
    ) : IRequest<PagedResponse<GetEstimateListResponseDto>>;
}
