using BBMB.BudgetService.Features.Employees.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Employees.Update
{
    public record UpdateEmployeeCommand(CreateEmployeeDto dto) : IRequest<Response>;
}
