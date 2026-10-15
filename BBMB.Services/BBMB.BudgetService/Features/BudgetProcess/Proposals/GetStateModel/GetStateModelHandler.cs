using BBMB.BudgetService.Features.BudgetProcess.Proposals.GetStateModel.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.GetStateModel
{
    /// <summary>Read-only. T-16 is read only when present (design §11.4), so the endpoint never fails with a 500.</summary>
    public sealed class GetStateModelHandler(BudgetContext context, IProposalTransitionService transitions)
        : IRequestHandler<GetStateModelQuery, Response<StateModelResponseDto>>
    {
        public async Task<Response<StateModelResponseDto>> Handle(GetStateModelQuery request, CancellationToken ct)
        {
            var dto = new StateModelResponseDto
            {
                States = await context.ProposalStates.AsNoTracking().OrderBy(s => s.StateCode)
                    .Select(s => new StateDto(s.StateCode, s.StateName, s.HolderLevel, s.FiguresOpen)).ToListAsync(ct),
                StateModelAvailable = await transitions.IsStateModelAvailableAsync(ct),
            };
            if (dto.StateModelAvailable)
                dto.Transitions = await context.StateTransitions.AsNoTracking()
                    .OrderBy(t => t.FromState).ThenBy(t => t.ActionCode).ThenBy(t => t.ToState)
                    .Select(t => new TransitionDto(t.FromState, t.ActionCode, t.ToState, t.RemarkRequired)).ToListAsync(ct);
            return new Response<StateModelResponseDto> { IsSucced = true, Message = "State model", Data = dto };
        }
    }
}
