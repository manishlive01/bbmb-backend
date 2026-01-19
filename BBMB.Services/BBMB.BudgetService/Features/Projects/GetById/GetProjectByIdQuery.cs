using BBMB.BudgetService.Features.Projects.GetById.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.Projects.GetById
{
    public sealed record GetProjectByIdQuery(string Code) : IRequest<GetProjectByIdResponseDto>;
}
