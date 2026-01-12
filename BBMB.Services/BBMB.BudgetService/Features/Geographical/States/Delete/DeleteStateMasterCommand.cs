using BBMB.BudgetService.Shared.RequestDTOs;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Geographical.States.Delete
{
    public sealed record DeleteStateMasterCommand(
        GetByIdRequestDto Request)
        : IRequest<Response>;
}
