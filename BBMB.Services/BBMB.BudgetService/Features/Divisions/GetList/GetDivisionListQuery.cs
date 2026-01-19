using BBMB.BudgetService.Features.Divisions.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Divisions.GetList
{
    public sealed record GetDivisionListQuery(
        int PageNumber = 1,
        int PageSize = 10,
        string? SortBy = null,
        bool SortDescending = false,
        int? DivisionCode = null,
        string? DivisionDesc = null,
        string? UserCode = null,
        string? RegionCode = null,
        string? PoolCode = null,
        DateTime? ModificationDateFrom = null,
        DateTime? ModificationDateTo = null
    ) : IRequest<PagedResponse<GetDivisionListResponseDto>>;
}
