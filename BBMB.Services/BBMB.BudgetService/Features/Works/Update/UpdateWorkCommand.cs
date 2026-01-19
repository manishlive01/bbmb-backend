using BBMB.BudgetService.Features.Works.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Works.Update
{
    public sealed record UpdateWorkCommand(string Code, UpdateWorkDto dto) : IRequest<Response>;
}
