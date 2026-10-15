using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Submit
{
    /// <summary>
    /// Design §8.3 Submit, §6.2 T01–T03 (SUBMIT from S-01) and T08–T08c (RESUBMIT from S-04); the target is chosen
    /// by the origin's level (Division → S-02, Circle → S-05, CE → S-07) in <see cref="TransitionResolver"/>.
    /// Inside the move's action scope, before anything is written, the line rule set runs at moment
    /// <see cref="RuleMoment.Submit"/> over every non-deleted line: any failure → <c>Response&lt;RuleFailuresDto&gt;</c>
    /// with every failing line and nothing moves (BUD-010, BUD-096). On success the flags the moment computed are
    /// written onto the lines (still at S-01 / S-04, figures open to the origin), then the movement.
    /// <c>first_submitted_at</c> is set on the first SUBMIT by the service. A head-wise proposal with no line
    /// submits as a nil demand (§6.2, OI-43). The right (F07.Submit / F11.Submit) is authorised by the service.
    /// </summary>
    public sealed class SubmitProposalHandler(
        BudgetContext context, IBudgetActorSource actorSource, IProposalTransitionService transitions, IDemandLineRules rules)
        : IRequestHandler<SubmitProposalCommand, Response>
    {
        public async Task<Response> Handle(SubmitProposalCommand request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail(BudgetMessages.M20);
            var state = await context.BudgetProposals.AsNoTracking()
                .Where(p => p.ProposalId == request.ProposalId).Select(p => p.StateCode).FirstOrDefaultAsync(ct);
            var action = state == ProposalStateCode.S04 ? ProposalAction.Resubmit : ProposalAction.Submit;
            var hooks = new TransitionHooks
            {
                BeforeMove = async (proposal, token) =>
                {
                    var lines = await context.DemandLines
                        .Where(l => l.ProposalId == proposal.ProposalId && !l.IsDeleted)
                        .OrderBy(l => l.SerialNo).ToListAsync(token);
                    var result = await rules.EvaluateAsync(proposal, lines, RuleMoment.Submit, actor.OfficeLevel, token);
                    if (result.Refused)
                        return BudgetResponses.RuleFailures(result.ToDto());
                    foreach (var f in result.Flags)
                        if (lines.FirstOrDefault(l => l.DemandLineId == f.DemandLineId) is { } line)
                        {
                            line.PctCheckNotApplied = f.PctCheckNotApplied;
                            line.FloorCheckUnchecked = f.FloorCheckUnchecked;
                        }
                    await context.SaveChangesAsync(token);
                    return null;
                },
            };
            return await transitions.MoveAsync(request.ProposalId, action, null, ct, hooks);
        }
    }
}
