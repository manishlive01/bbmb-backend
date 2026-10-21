using BBMB.BudgetService.Features.BudgetProcess.Proposals.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals
{
    /// <summary>
    /// Design §4.9 scope rule for a proposal addressed by id: absent, or neither its origin nor its holder in the
    /// actor's visible offices → null (the caller answers NOT_FOUND, never 403, BUD-031).
    /// </summary>
    public static class ProposalReads
    {
        public static async Task<BudgetProposal?> FindVisibleAsync(
            BudgetContext context, IBudgetOfficeScope officeScope, BudgetActor actor, long proposalId, bool track, CancellationToken ct)
        {
            var q = track ? context.BudgetProposals : context.BudgetProposals.AsNoTracking();
            var p = await q.FirstOrDefaultAsync(x => x.ProposalId == proposalId, ct);
            if (p is null)
                return null;
            var visible = await officeScope.GetVisibleOfficeIdsAsync(actor.OfficeId, ct);
            return visible.Contains(p.OriginOfficeId) || visible.Contains(p.HolderOfficeId) ? p : null;
        }
    }
}
