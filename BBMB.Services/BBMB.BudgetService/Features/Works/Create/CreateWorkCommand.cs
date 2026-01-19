using BBMB.BudgetService.Features.Works.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Works.Create
{
    public sealed record CreateWorkCommand(CreateWorkDto dto) : IRequest<Response>;
}
