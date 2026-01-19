using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Units.Delete
{
    public sealed record DeleteUnitCommand(string Code) : IRequest<Response>;
}
