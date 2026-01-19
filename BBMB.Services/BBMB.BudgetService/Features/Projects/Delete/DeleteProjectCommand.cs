using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Projects.Delete
{
    public sealed record DeleteProjectCommand(string Code) : IRequest<Response>;
}
