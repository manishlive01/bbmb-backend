using BBMB.BudgetService.Features.DetailHeads.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.DetailHeads.GetList
{
    public sealed record GetDetailHeadListQuery(
        int PageNumber = 1,
        int PageSize = 10,
        string? SortBy = null,
        bool SortDescending = false,
        string? DetailAccCode = null,
        string? MajorAccCode = null,
        string? SubMajorAccCode = null,
        string? MinorAccCode = null,
        string? SubMinorAccCode = null,
        string? DetailAccDesc = null,
        string? UserCode = null,
        DateTime? ModificationDateFrom = null,
        DateTime? ModificationDateTo = null
    ) : IRequest<PagedResponse<GetDetailHeadListResponseDto>>;
}
