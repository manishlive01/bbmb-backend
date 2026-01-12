using BBMB.BudgetService.Features.Geographical.States.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Geographical.States.Update
{
        public sealed record UpdateStateMasterCommand(UpdateStateMasterDto dto) : IRequest<Response>;
}
