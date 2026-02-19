using BBMB.BudgetService.Features.CEManagements.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.CEManagements.GetList
{
    public sealed record GetCEManagementListQuery(
        int PageNumber,
        int PageSize,
        string? SortBy,
        bool SortDescending,
        string? ProjectCode,
        bool? Status
    ) : IRequest<PagedResponse<GetCEManagementListResponseDto>>;
}
