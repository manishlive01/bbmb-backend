using BBMB.BudgetService.Features.Employees.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Employees.Create
{
   
    public record CreateEmployeeCommand(CreateEmployeeDto dto) : IRequest<Response>;
}
