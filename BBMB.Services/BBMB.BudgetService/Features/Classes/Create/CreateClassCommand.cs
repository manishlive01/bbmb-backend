using BBMB.BudgetService.Features.Classes.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Classes.Create
{
    public sealed record CreateClassCommand(CreateClassDto dto) : IRequest<Response>;
}
