using BBMB_Budget.Features.Geographical.States.Create.Dtos;
using BBMB_Budget.Shared.ResponseDTOs;
using MediatR;

namespace BBMB_Budget.Features.Geographical.States.Create
{
    public sealed record CreateStateMasterCommand(CreateStateMasterDto dto) : IRequest<Response>;
}
