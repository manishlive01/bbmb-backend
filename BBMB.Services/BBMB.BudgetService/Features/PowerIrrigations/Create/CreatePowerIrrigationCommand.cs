using BBMB.BudgetService.Features.PowerIrrigations.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.PowerIrrigations.Create
{
    public sealed record CreatePowerIrrigationCommand(CreatePowerIrrigationDto dto) : IRequest<Response>;
}
