using BBMB.BudgetService.Features.BudgetProcess.DemandLines;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Return
{
    /// <summary>
    /// Design §8.3 Return, §6.2 T06, T11, T11b, T15, T15b: one level down (DV-07; DB-12 refuses any other target).
    /// The selection names the lines whose observations go back with the proposal; null selects every line
    /// (BUD-106). A named line not of the proposal, or marked deleted, refuses the return (M-22 on
    /// selectedDemandLineIds) before anything moves. After the movement is written, inside the same RETURN scope,
    /// every unreturned observation of a selected line, and every unreturned foot remark (no line, BUD-128,
    /// which no selection can leave behind), gets <c>selected_for_return = true</c> and
    /// <c>returned_by_movement_id</c> = the movement (§5.5 row d, CR-DDD-04: the protect trigger permits exactly
    /// these two, once each, inside RETURN). Right Review.Examine.
    /// </summary>
    public sealed class ReturnProposalHandler(BudgetContext context, IProposalTransitionService transitions)
        : IRequestHandler<ReturnProposalCommand, Response>
    {
        public async Task<Response> Handle(ReturnProposalCommand request, CancellationToken ct)
        {
            var selected = request.Dto.SelectedDemandLineIds;
            var hooks = new TransitionHooks
            {
                BeforeMove = async (proposal, token) =>
                {
                    if (selected is null)
                        return null;
                    var live = await context.DemandLines.AsNoTracking()
                        .CountAsync(l => l.ProposalId == proposal.ProposalId && !l.IsDeleted && selected.Contains(l.DemandLineId), token);
                    return live == selected.Count
                        ? null
                        : BudgetResponses.RuleFailures(new RuleFailuresDto
                        {
                            Failures = [new RuleFailureItemDto(null, null, "selectedDemandLineIds", BudgetMessages.M22, BudgetMessages.Format(BudgetMessages.M22))],
                        });
                },
                AfterMove = async (proposal, movement, token) =>
                {
                    var observations = await context.Observations
                        .Where(o => o.ProposalId == proposal.ProposalId && o.ReturnedByMovementId == null &&
                                    (o.DemandLineId == null || selected == null || selected.Contains(o.DemandLineId.Value)))
                        .ToListAsync(token);
                    foreach (var o in observations)
                    {
                        o.SelectedForReturn = true;
                        o.ReturnedByMovementId = movement.MovementId;
                    }
                },
            };
            return await transitions.MoveAsync(request.ProposalId, ProposalAction.Return, request.Dto.Remark, ct, hooks);
        }
    }
}
