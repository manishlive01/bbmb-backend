using BBMB.BudgetService.Features.BudgetProcess.Cycles;
using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals
{
    /// <summary>
    /// The expected-office check of design §6.2 T20/T22 applied across every category present in a cycle: for
    /// WORKS, every active DIVISION office must have a proposal; for each head-wise category present, every
    /// office with a live T-21 assignment must have one (OI-43). Returns the offices that are missing a
    /// proposal in a category they are expected in (deduplicated and ordered). An empty list means the check
    /// passes.
    /// </summary>
    public static class PlacementExpectedSet
    {
        public static async Task<List<long>> MissingOfficesAsync(
            BudgetContext context, long cycleId, IReadOnlyList<BudgetProposal> cycleProposals, CancellationToken ct = default)
        {
            var missing = new HashSet<long>();

            // WORKS: every active DIVISION office is expected (BUD-134).
            if (cycleProposals.Any(p => p.Category == ProposalCategory.Works))
            {
                var divisions = await context.BudgetOffices.AsNoTracking()
                    .Where(o => o.IsActive && o.OfficeLevel == OfficeLevel.Division)
                    .Select(o => o.OfficeId).ToListAsync(ct);
                var worksOrigins = cycleProposals
                    .Where(p => p.Category == ProposalCategory.Works)
                    .Select(p => p.OriginOfficeId).ToHashSet();
                foreach (var d in divisions.Where(d => !worksOrigins.Contains(d)))
                    missing.Add(d);
            }

            // Each head-wise category present: every office with a live T-21 assignment is expected (OI-43).
            var headWiseCategories = cycleProposals
                .Where(p => ProposalCategory.HeadWise.Contains(p.Category))
                .Select(p => p.Category).Distinct().ToList();
            if (headWiseCategories.Count > 0)
            {
                var assigned = await HeadWiseExpectedSet.AssignedOfficesAsync(context, ct);
                foreach (var category in headWiseCategories)
                {
                    var origins = cycleProposals
                        .Where(p => p.Category == category)
                        .Select(p => p.OriginOfficeId).ToHashSet();
                    foreach (var o in assigned.Where(o => !origins.Contains(o)))
                        missing.Add(o);
                }
            }

            return missing.OrderBy(o => o).ToList();
        }
    }
}
