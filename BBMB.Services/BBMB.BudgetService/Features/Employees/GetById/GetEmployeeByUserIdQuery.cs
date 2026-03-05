using BBMB.BudgetService.Features.Employees.GetById.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.Employees.GetById
{
    public record GetEmployeeByUserIdQuery(Guid UserId)
       : IRequest<GetEmployeeResponseDto?>;
}
