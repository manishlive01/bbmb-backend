using BBMB.BudgetService.Features.BudgetProcess.Allocations.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Features.BudgetProcess.Provisions.Entities;
using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Allocations.Rules
{
    /// <summary>
    /// Design §8.6 F-19. Computes, from the finalised demand and the Chief Engineers' recommendations and
    /// distributions, the provisions an allocation of a body (BSC or FULL_BOARD) would create. The same
    /// computation feeds the preview (BUD-189) and the allocation itself, so the preview reflects exactly what
    /// the allocation commits; a difference between the two at allocation is the preview-mismatch refusal.
    ///
    /// BSC activates the revised estimate of the current year as RBE provisions (ALC-002); FULL_BOARD the
    /// original estimate of the next year as OBE provisions effective 1 April (BUD-182), and the revised
    /// estimate as RBE provisions that supersede the BSC provisions (BUD-285). The provision figures are the
    /// finalised demand-line figures (RBE / OBE_next) for WORK and UNFORESEEN holders, and the recommended
    /// figures spread by the distribution for head-wise holders (distribution shares plus the balance retained
    /// at the CE office, BUD-185).
    /// </summary>
    public sealed class AllocationRules(BudgetContext context)
    {
        public static string StateFor(string body) =>
            body == AllocationBody.Bsc ? ProposalStateCode.S12 : ProposalStateCode.S14;

        public static string AllocateActionFor(string body) =>
            body == AllocationBody.Bsc ? ProposalAction.AllocateRbe : ProposalAction.AllocateObe;

        public static string OccasionFor(string body) =>
            body == AllocationBody.Bsc ? WorkCodeOccasion.AllocRbe : WorkCodeOccasion.AllocObe;

        /// <summary>
        /// Computes the plan for the cycle and body. Returns null when a proposal of the cycle is not at the
        /// body's placement state (the caller lists the outstanding proposals and refuses, M-19).
        /// </summary>
        public async Task<AllocationComputation?> ComputeAsync(long cycleId, string body, CancellationToken ct = default)
        {
            var cycle = await context.BudgetCycles.AsNoTracking()
                .Where(c => c.CycleId == cycleId)
                .Select(c => new { c.CurrentFinYear, c.NextFinYear }).SingleAsync(ct);

            var requiredState = StateFor(body);
            var proposals = await context.BudgetProposals.AsNoTracking()
                .Where(p => p.CycleId == cycleId)
                .Select(p => new { p.ProposalId, p.Category, p.OriginOfficeId, p.StateCode })
                .ToListAsync(ct);
            if (proposals.Count == 0 || proposals.Any(p => p.StateCode != requiredState))
                return null;

            var provisions = new List<PlannedProvision>();
            var worksToCode = new List<int>();

            // --- WORK and UNFORESEEN demand lines: one provision each, from the finalised line figure. ---
            var proposalIds = proposals.Select(p => p.ProposalId).ToList();
            var lines = await context.DemandLines.AsNoTracking()
                .Where(l => proposalIds.Contains(l.ProposalId) && !l.IsDeleted &&
                            (l.LineType == DemandLineType.Work || l.LineType == DemandLineType.Unforeseen))
                .Select(l => new { l.DemandLineId, l.ProposalId, l.LineType, l.WorkId, l.AccountCodeId, l.RbeAmount, l.ObeNextAmount })
                .ToListAsync(ct);
            var proposalById = proposals.ToDictionary(p => p.ProposalId);
            // Works that already carry a code receive none again (BUD-286); a second (FULL_BOARD) allocation of
            // an already-coded work generates no second code.
            var codedWorkIds = (await context.Works.AsNoTracking()
                .Where(w => w.WorkCode != null).Select(w => w.Id).ToListAsync(ct)).ToHashSet();

            foreach (var l in lines)
            {
                var p = proposalById[l.ProposalId];
                var holderType = l.LineType; // WORK or UNFORESEEN
                var budgetaryCheck = p.Category != ProposalCategory.Receipts;

                if (body == AllocationBody.Bsc)
                {
                    provisions.Add(LineProvision(l.DemandLineId, l.ProposalId, holderType, ProvisionBasis.Rbe,
                        p.Category, cycle.CurrentFinYear, l.WorkId, p.OriginOfficeId, l.AccountCodeId,
                        l.RbeAmount ?? 0, budgetaryCheck));
                }
                else
                {
                    // FULL_BOARD: OBE for the next year, and the RBE as altered superseding the BSC RBE.
                    provisions.Add(LineProvision(l.DemandLineId, l.ProposalId, holderType, ProvisionBasis.Obe,
                        p.Category, cycle.NextFinYear, l.WorkId, p.OriginOfficeId, l.AccountCodeId,
                        l.ObeNextAmount ?? 0, budgetaryCheck));
                    provisions.Add(LineProvision(l.DemandLineId, l.ProposalId, holderType, ProvisionBasis.Rbe,
                        p.Category, cycle.CurrentFinYear, l.WorkId, p.OriginOfficeId, l.AccountCodeId,
                        l.RbeAmount ?? 0, budgetaryCheck));
                }

                if (holderType == DemandLineType.Work && l.WorkId is { } wid && !codedWorkIds.Contains(wid)
                    && !worksToCode.Contains(wid))
                    worksToCode.Add(wid);
            }

            // --- Head-wise holders: distribution shares and the CE balance retained (BUD-185). ---
            var headWiseProposals = proposals.Where(p => ProposalCategory.HeadWise.Contains(p.Category)).ToList();
            if (headWiseProposals.Count > 0)
            {
                var recommendations = await context.CeRecommendations.AsNoTracking()
                    .Where(r => r.CycleId == cycleId && (r.RbeRecommended != null || r.ObeNextRecommended != null))
                    .Select(r => new { r.RecommendationId, r.Category, r.CeOfficeId, r.AccountCodeId, r.RbeRecommended, r.ObeNextRecommended })
                    .ToListAsync(ct);
                var recIds = recommendations.Select(r => r.RecommendationId).ToList();
                var distributions = await context.Distributions.AsNoTracking()
                    .Where(d => recIds.Contains(d.RecommendationId))
                    .Select(d => new { d.DistributionId, d.RecommendationId, d.OfficeId, d.RbeAmount, d.ObeNextAmount })
                    .ToListAsync(ct);

                // The head-wise proposal of a category is framed by the CE office itself; its id anchors the
                // ALLOCATE transition. Map (category, ce office) -> proposal id.
                var headWiseProposalByKey = headWiseProposals
                    .GroupBy(p => (p.Category, p.OriginOfficeId))
                    .ToDictionary(g => g.Key, g => g.First().ProposalId);

                foreach (var r in recommendations)
                {
                    if (!headWiseProposalByKey.TryGetValue((r.Category, r.CeOfficeId), out var proposalId))
                        continue; // a recommendation without a matching head-wise proposal is not activated here
                    var budgetaryCheck = r.Category != ProposalCategory.Receipts;
                    var shares = distributions.Where(d => d.RecommendationId == r.RecommendationId).ToList();

                    foreach (var share in shares)
                        AddHeadProvisions(provisions, body, r.Category, cycle, share.OfficeId, r.AccountCodeId,
                            r.RecommendationId, share.DistributionId, share.RbeAmount, share.ObeNextAmount,
                            budgetaryCheck, proposalId);

                    // The balance retained at the CE office (recommended minus distributed), without a distribution id.
                    var rbeBalance = (r.RbeRecommended ?? 0) - shares.Sum(s => s.RbeAmount);
                    var obeBalance = (r.ObeNextRecommended ?? 0) - shares.Sum(s => s.ObeNextAmount);
                    if (rbeBalance != 0 || obeBalance != 0)
                        AddHeadProvisions(provisions, body, r.Category, cycle, r.CeOfficeId, r.AccountCodeId,
                            r.RecommendationId, null, rbeBalance, obeBalance, budgetaryCheck, proposalId);
                }
            }

            return new AllocationComputation(provisions, proposalIds, worksToCode);
        }

        private static void AddHeadProvisions(
            List<PlannedProvision> provisions, string body, string category,
            dynamic cycle, long officeId, long accountCodeId, long recommendationId, long? distributionId,
            decimal rbe, decimal obeNext, bool budgetaryCheck, long proposalId)
        {
            if (body == AllocationBody.Bsc)
            {
                provisions.Add(new PlannedProvision(ProvisionHolderType.Head, ProvisionBasis.Rbe, category,
                    (int)cycle.CurrentFinYear, null, officeId, accountCodeId, null, recommendationId, distributionId,
                    rbe, budgetaryCheck, proposalId));
            }
            else
            {
                provisions.Add(new PlannedProvision(ProvisionHolderType.Head, ProvisionBasis.Obe, category,
                    (int)cycle.NextFinYear, null, officeId, accountCodeId, null, recommendationId, distributionId,
                    obeNext, budgetaryCheck, proposalId));
                provisions.Add(new PlannedProvision(ProvisionHolderType.Head, ProvisionBasis.Rbe, category,
                    (int)cycle.CurrentFinYear, null, officeId, accountCodeId, null, recommendationId, distributionId,
                    rbe, budgetaryCheck, proposalId));
            }
        }

        private static PlannedProvision LineProvision(
            long demandLineId, long proposalId, string holderType, string basis, string category, int finYear,
            int? workId, long officeId, long? accountCodeId, decimal amount, bool budgetaryCheck) =>
            new(holderType, basis, category, finYear, workId, officeId, accountCodeId, demandLineId, null, null,
                amount, budgetaryCheck, proposalId);
    }
}
