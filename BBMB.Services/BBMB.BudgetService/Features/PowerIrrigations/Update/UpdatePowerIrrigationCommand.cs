using BBMB.BudgetService.Features.PowerIrrigations.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.PowerIrrigations.Update
{
    public sealed record UpdatePowerIrrigationCommand(string Code, UpdatePowerIrrigationDto dto) : IRequest<Response>;
}
