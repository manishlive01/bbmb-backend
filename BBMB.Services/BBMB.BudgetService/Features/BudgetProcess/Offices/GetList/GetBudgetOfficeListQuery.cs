using BBMB.BudgetService.Features.BudgetProcess.Offices.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Offices.GetList
{
    public sealed record GetBudgetOfficeListQuery(int PageNumber, int PageSize, string? Level, long? ParentId)
        : IRequest<PagedResponse<GetBudgetOfficeListResponseDto>>;
}
