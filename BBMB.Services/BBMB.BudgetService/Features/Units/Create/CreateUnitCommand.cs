using BBMB.BudgetService.Features.Units.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Units.Create
{
    public sealed record CreateUnitCommand(CreateUnitDto dto) : IRequest<Response>;
}
