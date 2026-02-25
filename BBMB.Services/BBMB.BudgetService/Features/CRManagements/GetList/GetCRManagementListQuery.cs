using BBMB.BudgetService.Features.CRManagements.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.CRManagements.GetList
{
    public sealed record GetCRManagementListQuery(
        int PageNumber,
        int PageSize,
        string? SortBy,
        bool SortDescending,
        string? crCode,
        string? ceCode,
        bool? Status
    ) : IRequest<PagedResponse<GetCRManagementListResponseDto>>;
}
