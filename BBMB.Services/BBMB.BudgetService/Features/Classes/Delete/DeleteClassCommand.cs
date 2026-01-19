using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Classes.Delete
{
    public sealed record DeleteClassCommand(string Code) : IRequest<Response>;
}
