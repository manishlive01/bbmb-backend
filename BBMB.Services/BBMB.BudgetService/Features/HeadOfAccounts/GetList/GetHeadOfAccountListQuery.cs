using BBMB.BudgetService.Features.HeadOfAccounts.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.HeadOfAccounts.GetList
{
    public sealed record GetHeadOfAccountListQuery(
        int PageNumber,
        int PageSize,
        string? SortBy,
        bool SortDescending,
        string? MajorAccCode,
        string? SubMajorAccCode,
        string? MinorAccCode,
        string? SubMinorAccCode,
        string? DetailAccCode,
        string? ObjectAccCode,
        string? HeadDesc,
        DateTime? ModificationDateFrom,
        DateTime? ModificationDateTo
    ) : IRequest<PagedResponse<GetHeadOfAccountListResponseDto>>;
}
