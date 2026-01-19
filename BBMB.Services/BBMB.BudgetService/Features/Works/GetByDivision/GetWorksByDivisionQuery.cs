using BBMB.BudgetService.Features.Works.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Works.GetByDivision
{
    public sealed record GetWorksByDivisionQuery(
        int DivisionCode,
        int PageNumber,
        int PageSize
    ) : IRequest<PagedResponse<GetWorkListResponseDto>>;
}
