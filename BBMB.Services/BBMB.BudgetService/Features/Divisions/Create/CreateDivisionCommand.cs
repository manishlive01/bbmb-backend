using BBMB.BudgetService.Features.Divisions.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Divisions.Create
{
    public sealed record CreateDivisionCommand(CreateDivisionDto dto) : IRequest<Response>;
}
