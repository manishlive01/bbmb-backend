using BBMB.BudgetService.Features.Works.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Works.GetList
{
    public sealed record GetWorkListQuery(
        int PageNumber,
        int PageSize,
        string? SortBy,
        bool SortDescending,
        string? WorkCode,
        string? WorkDesc,
        int? DivisionCode,
        string? Status,
        string? Nature
    ) : IRequest<PagedResponse<GetWorkListResponseDto>>;
}
