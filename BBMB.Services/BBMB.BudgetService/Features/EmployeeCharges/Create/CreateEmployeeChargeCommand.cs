using BBMB.BudgetService.Features.EmployeeCharges.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.EmployeeCharges.Create
{
    public record CreateEmployeeChargeCommand(CreateEmployeeChargeDto dto)
        : IRequest<Response>;
}
