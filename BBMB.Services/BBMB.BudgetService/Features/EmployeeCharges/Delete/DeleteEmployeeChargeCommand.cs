using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.EmployeeCharges.Delete
{
    public record DeleteEmployeeChargeCommand(Guid Id) : IRequest<Response>;
}