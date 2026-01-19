using BBMB.BudgetService.Features.MinorHeads.SubMinor.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.MinorHeads.SubMinor.GetList
{
    public record GetSubMinorHeadListQuery(
        string? SubMinorAccCode = null,
        string? MajorAccCode = null,
        string? SubMajorAccCode = null,
        string? MinorAccCode = null,
        string? SubMinorAccDesc = null,
        string? UserCode = null,
        DateTime? ModificationDateFrom = null,
        DateTime? ModificationDateTo = null,
        string? SortBy = null,
        bool SortDescending = false,
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<PagedResponse<GetSubMinorHeadListResponseDto>>;
}
