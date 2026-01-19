using BBMB.BudgetService.Features.Pools.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Pools.Update
{
    public sealed record UpdatePoolCommand(string Code, UpdatePoolDto dto) : IRequest<Response>;
}
