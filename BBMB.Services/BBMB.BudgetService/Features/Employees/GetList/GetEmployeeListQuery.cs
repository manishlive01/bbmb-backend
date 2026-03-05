using BBMB.BudgetService.Features.Employees.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Employees.GetList
{
    public sealed record GetEmployeeListQuery(GetEmployeeListRequestDto dto)
      : IRequest<PagedResponse<GetEmployeeListResponseDto>>;
}
