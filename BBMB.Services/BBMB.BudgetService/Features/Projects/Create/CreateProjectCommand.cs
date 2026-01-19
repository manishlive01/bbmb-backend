using BBMB.BudgetService.Features.Projects.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Projects.Create
{
    public sealed record CreateProjectCommand(CreateProjectDto dto) : IRequest<Response>;
}
