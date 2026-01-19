using BBMB.BudgetService.Features.Units.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Units.Update
{
    public sealed record UpdateUnitCommand(string Code, UpdateUnitDto dto) : IRequest<Response>;
}
