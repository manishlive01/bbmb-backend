using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Works.Delete
{
    public sealed record DeleteWorkCommand(string Code) : IRequest<Response>;
}
