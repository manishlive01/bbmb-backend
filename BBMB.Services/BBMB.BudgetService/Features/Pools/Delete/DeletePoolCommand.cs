using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Pools.Delete
{
    public sealed record DeletePoolCommand(string Code) : IRequest<Response>;
}
