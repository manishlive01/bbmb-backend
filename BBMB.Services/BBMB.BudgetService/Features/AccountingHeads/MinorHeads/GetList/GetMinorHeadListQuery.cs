using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetList
{
    public sealed record GetMinorHeadListQuery(
        int PageNumber,
        int PageSize,
        string? SortBy,
        bool SortDescending,
        string? MinorAccCode,
        string? MajorAccCode,
        string? SubMajorAccCode,
        string? MinorAccDesc,
        string? UserCode,
        DateTime? ModificationDateFrom,
        DateTime? ModificationDateTo
    ) : IRequest<PagedResponse<GetMinorHeadListResponseDto>>;
}
