using BBMB_Budget.Features.Geographical.States.Update.Dtos;
using BBMB_Budget.Shared.ResponseDTOs;
using MediatR;

namespace BBMB_Budget.Features.Geographical.States.Update
{
        public sealed record UpdateStateMasterCommand(UpdateStateMasterDto dto) : IRequest<Response>;
}
