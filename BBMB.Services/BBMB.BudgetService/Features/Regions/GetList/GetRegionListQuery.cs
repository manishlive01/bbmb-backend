using BBMB.BudgetService.Features.Regions.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Regions.GetList
{
    public sealed record GetRegionListQuery(
        int PageNumber = 1,
        int PageSize = 10,
        string? SortBy = null,
        bool SortDescending = false,
        string? RegionCode = null,
        string? RegionDesc = null,
        string? UserCode = null,
        DateTime? ModificationDateFrom = null,
        DateTime? ModificationDateTo = null
    ) : IRequest<PagedResponse<GetRegionListResponseDto>>;
}
