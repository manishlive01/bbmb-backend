using BBMB.BudgetService.Features.Units.GetById.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.Units.GetById
{
    public sealed record GetUnitByIdQuery(string UnitCode) : IRequest<GetUnitByIdResponseDto>;
}
