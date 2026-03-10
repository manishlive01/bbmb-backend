using BBMB.BudgetService.Features.EmployeeCharges.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.EmployeeCharges.Update
{
    public record UpdateEmployeeChargeCommand(
        UpdateEmployeeChargeDto dto
    ) : IRequest<Response>;
}