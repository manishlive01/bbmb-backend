using BBMB.BudgetService.Features.BudgetProcess.CorrectionFlags.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.ReferBack
{
    /// <summary>
    /// Design §6.2 T24 (S-12 → S-08) and T27 (S-14 → S-08), §8.6 refer-back. Per proposal, to the Secretariat,
    /// with a remark (M-12). The right (F19.Allocate), holder, cycle and remark checks are the transition
    /// service's. T27 only: a reference back from the Full Board raises a <c>PROPOSAL_REOPENED</c> correction
    /// flag (BUD-132) on each live provision of the proposal, so the change of basis is surfaced for correction;
    /// the flag insert runs inside the same action scope through the AfterMove hook. The §6.2 known limitation
    /// (a referred-back proposal holds the cycle short of the Full Board allocation) is a consequence of this,
    /// not handled here.
    /// </summary>
    public sealed class ReferBackHandler(BudgetContext context, IProposalTransitionService transitions)
        : IRequestHandler<ReferBackCommand, Response>
    {
        public Task<Response> Handle(ReferBackCommand request, CancellationToken ct) =>
            transitions.MoveAsync(request.ProposalId, ProposalAction.ReferBack, request.Remark, ct,
                new TransitionHooks
                {
                    AfterMove = async (proposal, movement, token) =>
                    {
                        // T27 only: the proposal came back from the Full Board (S-14). Raise PROPOSAL_REOPENED
                        // on every live provision whose demand line belongs to this proposal (BUD-132).
                        if (movement.FromState != ProposalStateCode.S14)
                            return;

                        var liveProvisions = await (
                            from pr in context.Provisions
                            join dl in context.DemandLines on pr.DemandLineId equals dl.DemandLineId
                            where dl.ProposalId == proposal.ProposalId && pr.SupersededByProvisionId == null
                            select new { pr.ProvisionId, pr.WorkId }).ToListAsync(token);

                        foreach (var pr in liveProvisions)
                        {
                            // One PROPOSAL_REOPENED per live provision; skip one already standing uncleared.
                            var exists = await context.CorrectionFlags.AnyAsync(f =>
                                f.ProvisionId == pr.ProvisionId &&
                                f.Reason == CorrectionFlagReason.ProposalReopened &&
                                f.ClearedAt == null, token);
                            if (exists)
                                continue;
                            context.CorrectionFlags.Add(new CorrectionFlag
                            {
                                Reason = CorrectionFlagReason.ProposalReopened,
                                WorkId = pr.WorkId,
                                ProvisionId = pr.ProvisionId,
                                IsCritical = false,
                            });
                        }
                    },
                });
    }
}
