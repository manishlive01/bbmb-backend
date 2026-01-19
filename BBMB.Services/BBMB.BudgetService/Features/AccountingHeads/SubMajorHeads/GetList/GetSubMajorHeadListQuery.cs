using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetList
{
    public sealed record GetSubMajorHeadListQuery(
        int PageNumber,
        int PageSize,
        string? SortBy,
        bool SortDescending,
        string? SubMajorAccCode,
        string? MajorAccCode,
        string? SubMajorAccDesc,
        string? UserCode,
        DateTime? ModificationDateFrom,
        DateTime? ModificationDateTo
    ) : IRequest<PagedResponse<GetSubMajorHeadListResponseDto>>;
}
