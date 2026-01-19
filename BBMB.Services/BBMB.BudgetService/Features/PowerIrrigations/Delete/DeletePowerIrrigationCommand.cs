using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.PowerIrrigations.Delete
{
    public sealed record DeletePowerIrrigationCommand(string Code) : IRequest<Response>;
}
