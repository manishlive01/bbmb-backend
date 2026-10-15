using BBMB.BudgetService.Features.BudgetProcess.Cycles.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Distributions.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Distributions.Rules;
using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Distributions.Save
{
    /// <summary>
    /// Design §8.5 Save (F-14, BUD-156, BUD-157, DNA-010). Order: actor (M-20) → the recommendation exists in
    /// the cycle, else NOT_FOUND → the actor's office is the recommendation's CE, else M-20 (BUD-156) → the
    /// cycle is OPEN (not frozen), else M-17 → every proposal of the category under the CE is at S-11 or later,
    /// else M-19 (DNA-010) → the per-column total of the proposed shares is within the recommended figure
    /// (VR-07, M-14), else refused → the shares are upserted on (recommendation, office) in one
    /// <c>DISTRIBUTION_SAVE</c> scope; the DB-18 constraint re-checks the set at commit, and DB-01 refuses an
    /// office outside the CE's charge.
    /// </summary>
    public sealed class SaveDistributionHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope)
        : IRequestHandler<SaveDistributionCommand, Response>
    {
        public const string ActionCode = "DISTRIBUTION_SAVE";

        /// <summary>S-11 and later: distribution begins once Finance has recommended (design §5 S-11, DNA-010).</summary>
        private static readonly string[] S11OrLater =
        [
            ProposalStateCode.S11, ProposalStateCode.S12, ProposalStateCode.S13,
            ProposalStateCode.S14, ProposalStateCode.S15, ProposalStateCode.S16,
        ];

        public async Task<Response> Handle(SaveDistributionCommand request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail(BudgetMessages.M20);
            var dto = request.Dto;
            var rec = await context.CeRecommendations.AsNoTracking()
                .FirstOrDefaultAsync(r => r.RecommendationId == dto.RecommendationId && r.CycleId == request.CycleId, ct);
            if (rec is null)
                return BudgetResponses.Fail(BudgetMessages.NotFound);
            if (rec.CeOfficeId != actor.OfficeId)
                return BudgetResponses.Fail(BudgetMessages.M20); // only the recommendation's CE distributes (BUD-156)

            var cycleStatus = await context.BudgetCycles.AsNoTracking()
                .Where(c => c.CycleId == request.CycleId).Select(c => c.Status).SingleAsync(ct);
            if (cycleStatus != CycleStatus.Open)
                return BudgetResponses.Fail(BudgetMessages.M17);

            // Every proposal of the category under the CE must be at S-11 or later (DNA-010).
            var circles = context.BudgetOffices.AsNoTracking()
                .Where(o => o.ParentOfficeId == rec.CeOfficeId && o.OfficeLevel == OfficeLevel.Circle).Select(o => o.OfficeId);
            var underCe = context.BudgetOffices.AsNoTracking()
                .Where(o => o.OfficeId == rec.CeOfficeId || circles.Contains(o.OfficeId) ||
                            (o.OfficeLevel == OfficeLevel.Division && o.ParentOfficeId != null && circles.Contains(o.ParentOfficeId.Value)))
                .Select(o => o.OfficeId);
            var states = await context.BudgetProposals.AsNoTracking()
                .Where(p => p.CycleId == request.CycleId && p.Category == rec.Category && underCe.Contains(p.OriginOfficeId))
                .Select(p => p.StateCode).ToListAsync(ct);
            if (states.Count == 0 || states.Any(s => !S11OrLater.Contains(s)))
                return BudgetResponses.Fail(BudgetMessages.M19);

            // VR-07 pre-check: the proposed set's per-column totals within the recommended figure (M-14). The
            // DB-18 deferred constraint re-checks the committed set, so this is the friendly refusal, not the guard.
            var rbeTotal = dto.Lines.Sum(l => l.RbeAmount);
            var obeTotal = dto.Lines.Sum(l => l.ObeNextAmount);
            if (DistributionRules.OverColumn(rec.RbeRecommended, rec.ObeNextRecommended, rbeTotal, obeTotal) is { } overField)
                return new Response
                {
                    IsSucced = false, MessageId = BudgetMessages.M14,
                    Message = $"{BudgetMessages.Format(BudgetMessages.M14)} ({overField})",
                };

            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT 1 FROM budget.recommendation WHERE recommendation_id = {dto.RecommendationId} FOR UPDATE", ct);
            var existing = await context.Distributions
                .Where(d => d.RecommendationId == dto.RecommendationId).ToListAsync(ct);
            foreach (var line in dto.Lines)
            {
                var row = existing.FirstOrDefault(d => d.OfficeId == line.OfficeId);
                if (row is null)
                {
                    context.Distributions.Add(new Distribution
                    {
                        RecommendationId = dto.RecommendationId, OfficeId = line.OfficeId,
                        RbeAmount = line.RbeAmount, ObeNextAmount = line.ObeNextAmount,
                    });
                }
                else
                {
                    row.RbeAmount = line.RbeAmount;
                    row.ObeNextAmount = line.ObeNextAmount;
                }
            }
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new Response { IsSucced = true, Message = "Distribution saved" };
        }
    }
}
