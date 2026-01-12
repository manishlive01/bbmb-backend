using BBMB.BudgetService.Features.Geographical.States.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Geographical.States.Create
{
    public sealed record CreateStateMasterCommand(CreateStateMasterDto dto) : IRequest<Response>;
}
