using BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.GetList
{
    public sealed record GetDivSectionManagementListQuery(
        int PageNumber,
        int PageSize,
        string? SortBy,
        bool SortDescending,
        int? DivisionId,
        int? DivisionCode,
        string? DivisionName,
        bool? Status
    ) : IRequest<PagedResponse<GetDivSectionManagementListResponseDto>>;
}