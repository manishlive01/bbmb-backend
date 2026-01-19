using BBMB.BudgetService.Features.PowerIrrigations.GetById.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.PowerIrrigations.GetById
{
    public sealed record GetPowerIrrigationByIdQuery(string Code) : IRequest<GetPowerIrrigationByIdResponseDto>;
}
