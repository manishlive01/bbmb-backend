using BBMB.BudgetService.Features.Projects.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Projects.Update
{
    public sealed record UpdateProjectCommand(string Code, UpdateProjectDto dto) : IRequest<Response>;
}
