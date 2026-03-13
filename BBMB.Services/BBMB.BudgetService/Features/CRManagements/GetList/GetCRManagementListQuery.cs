using BBMB.BudgetService.Features.CRManagements.GetList.Dtos;
using BBMB.BudgetService.Shared.RequestDTOs;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.CRManagements.GetList
{
    public sealed record GetCRManagementListQuery(
        int PageNumber,
        int PageSize,
        string? SortBy,
        bool SortDescending,
        int? crCode,
        int? ceCode,
        bool? Status
    ) : IRequest<PagedResponse<GetCRManagementListResponseDto>>;

    public sealed record GetCRManagementListById(GetByIdRequestDto RequestDto
    ) : IRequest<List<GetCRManagementListResponseDto>>;
}
