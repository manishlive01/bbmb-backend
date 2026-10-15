using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Forward
{
    /// <summary>
    /// Design §8.3 Forward, §6.2 T07 (Circle, S-03 → S-05) and T12 (CE, S-06 → S-07). Inside the move's scope the
    /// line rule set runs at moment <see cref="RuleMoment.ReviewerForward"/> over every non-deleted line (the
    /// reviewer forwards only a complete demand, BUD-113); any failure → <c>Response&lt;RuleFailuresDto&gt;</c> and
    /// nothing moves. Right Review.Examine.
    /// </summary>
    public sealed class ForwardProposalHandler(
        BudgetContext context, IBudgetActorSource actorSource, IProposalTransitionService transitions, IDemandLineRules rules)
        : IRequestHandler<ForwardProposalCommand, Response>
    {
        public async Task<Response> Handle(ForwardProposalCommand request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail(BudgetMessages.M20);
            var hooks = new TransitionHooks
            {
                BeforeMove = async (proposal, token) =>
                {
                    var lines = await context.DemandLines.AsNoTracking()
                        .Where(l => l.ProposalId == proposal.ProposalId && !l.IsDeleted)
                        .OrderBy(l => l.SerialNo).ToListAsync(token);
                    var result = await rules.EvaluateAsync(proposal, lines, RuleMoment.ReviewerForward, actor.OfficeLevel, token);
                    return result.Refused ? BudgetResponses.RuleFailures(result.ToDto()) : null;
                },
            };
            return await transitions.MoveAsync(request.ProposalId, ProposalAction.Forward, request.Remark, ct, hooks);
        }
    }
}
