using BBMB.BudgetService.Features.Units.GetByPowerIrrigation.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Units.GetByPowerIrrigation
{
    public sealed record GetUnitsByPowerIrrigationQuery(string IrrPwrCode) : IRequest<Response<List<GetUnitsByPowerIrrigationResponseDto>>>;
}
