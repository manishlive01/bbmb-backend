using BBMB.BudgetService.Features.Classes.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Classes.Update
{
    public sealed record UpdateClassCommand(string Code, UpdateClassDto dto) : IRequest<Response>;
}
