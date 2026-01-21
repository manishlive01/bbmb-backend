using BBMB.BudgetService.Features.Offices.AccountOffices.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Offices.AccountOffices.GetList
{
    public sealed record GetAccountOfficeListQuery(
        int PageNumber = 1,
        int PageSize = 10,
        string? SortBy = null,
        bool SortDescending = false,
        string? AccOffice = null,
        string? AccOfficeDesc = null,
        string? UserCode = null,
        DateTime? ModificationDateFrom = null,
        DateTime? ModificationDateTo = null
    ) : IRequest<PagedResponse<GetAccountOfficeListResponseDto>>;
}
