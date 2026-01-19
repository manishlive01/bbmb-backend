using BBMB.BudgetService.Features.Pools.GetById.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.Pools.GetById
{
    public sealed record GetPoolByIdQuery(string Code) : IRequest<GetPoolByIdResponseDto>;
}
