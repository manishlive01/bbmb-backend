using BBMB_Budget.Shared.RequestDTOs;
using BBMB_Budget.Shared.ResponseDTOs;
using MediatR;

namespace BBMB_Budget.Features.Geographical.States.Delete
{
    public sealed record DeleteStateMasterCommand(
        GetByIdRequestDto Request)
        : IRequest<Response>;
}
