using BBMB.BudgetService.Features.Offices.OfficeManagements.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Offices.OfficeManagements.GetList
{
    public sealed record GetOfficeManagementListQuery(
        int PageNumber = 1,
        int PageSize = 10,
        string? SortBy = null,
        bool SortDescending = false,
        string? OfficeCode = null,
        string? OfficeDesc = null,
        int? CECode = null,
        int? CRCode = null,
        int? DivisionCode = null,
        string? UserCode = null,
        DateTime? ModificationDateFrom = null,
        DateTime? ModificationDateTo = null
    ) : IRequest<PagedResponse<GetOfficeManagementListResponseDto>>;
}