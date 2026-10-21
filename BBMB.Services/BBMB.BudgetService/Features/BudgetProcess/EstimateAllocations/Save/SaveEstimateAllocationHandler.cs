using BBMB.BudgetService.Features.BudgetProcess.EstimateAllocations.Entities;
using BBMB.BudgetService.Features.BudgetProcess.EstimateAllocations.Rules;
using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.EstimateAllocations.Save
{
    /// <summary>
    /// Design §8.6 PUT estimate-allocations (F-29, VR-12 / M-28; BUD-288..291, BUD-309). Order, nothing written
    /// on any refusal:
    /// <list type="number">
    /// <item>actor (M-20); the actor's Division;</item>
    /// <item>the destination provision is a live allocated WORK provision of the Division, and no critical
    /// correction flag stands (<see cref="EstimateAllocationRules.ResolveTargetAsync"/>, NOT_FOUND / M-19);</item>
    /// <item>every line names a technical sanction of the work that carries an estimate code (DB-04), else M-05;
    /// amounts ≥ 0 (validator); the total does not exceed the provision available (M-28);</item>
    /// <item>the T-43 rows are upserted in one F29_ALLOCATE scope (DB-23 re-checks at commit); when the total now
    /// equals the available (VR-12), the work's REALLOCATE_ESTIMATES flags are cleared.</item>
    /// </list>
    /// Right F29.Allocate (endpoint).
    /// </summary>
    public sealed class SaveEstimateAllocationHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, EstimateAllocationRules rules)
        : IRequestHandler<SaveEstimateAllocationCommand, Response>
    {
        public async Task<Response> Handle(SaveEstimateAllocationCommand request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail(BudgetMessages.M20);
            var actorDivisionId = await context.BudgetOffices.AsNoTracking()
                .Where(o => o.OfficeId == actor.OfficeId && o.OfficeLevel == OfficeLevel.Division)
                .Select(o => o.DivisionId).FirstOrDefaultAsync(ct);
            if (actorDivisionId is null)
                return BudgetResponses.Fail(BudgetMessages.M20);

            var (target, failure) = await rules.ResolveTargetAsync(request.ProvisionId, actorDivisionId.Value, ct);
            if (target is null)
                return BudgetResponses.Fail(failure!);

            var lines = request.Dto.Lines;
            // Every named sanction is of the work and carries an estimate code (DB-04); duplicates are refused.
            var validSanctions = await rules.SanctionsWithEstimateCodeAsync(target.WorkId, ct);
            if (lines.Select(l => l.TechnicalSanctionId).Distinct().Count() != lines.Count)
                return BudgetResponses.Fail(BudgetMessages.M19);
            if (lines.Any(l => !validSanctions.ContainsKey(l.TechnicalSanctionId)))
                return BudgetResponses.Fail(BudgetMessages.M05);
            if (lines.Any(l => l.Amount < 0))
                return BudgetResponses.Fail(BudgetMessages.M22);

            var total = lines.Sum(l => l.Amount);
            if (total > target.Available)
                return BudgetResponses.Fail(BudgetMessages.M28);

            await using var tx = await scope.BeginAsync("F29_ALLOCATE", ct: ct);

            var existing = await context.EstimateAllocations
                .Where(e => e.ProvisionId == target.ProvisionId).ToListAsync(ct);
            var bySanction = existing.ToDictionary(e => e.TechnicalSanctionId);
            var sent = lines.ToDictionary(l => l.TechnicalSanctionId, l => l.Amount);

            foreach (var line in lines)
            {
                if (bySanction.TryGetValue(line.TechnicalSanctionId, out var row))
                    row.Amount = line.Amount;
                else
                    context.EstimateAllocations.Add(new EstimateAllocation
                    {
                        ProvisionId = target.ProvisionId,
                        TechnicalSanctionId = line.TechnicalSanctionId,
                        Amount = line.Amount,
                    });
            }
            // A sanction omitted from the set is set to zero (T-43 is append-only in effect: no delete, DB-13).
            foreach (var row in existing.Where(e => !sent.ContainsKey(e.TechnicalSanctionId)))
                row.Amount = 0;

            // VR-12 (BUD-291): once the work's estimate allocation uses up the available, clear REALLOCATE_ESTIMATES.
            if (EstimateAllocationRules.Vr12Satisfied(total, target.Available))
            {
                var flags = await context.CorrectionFlags
                    .Where(f => f.WorkId == target.WorkId && f.Reason == rules.ReallocateEstimatesReason && f.ClearedAt == null)
                    .ToListAsync(ct);
                foreach (var flag in flags)
                {
                    flag.ClearedAt = DateTimeOffset.UtcNow;
                    flag.ClearedBy = actor.EmployeeId;
                }
            }

            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new Response { IsSucced = true, Message = "Estimate allocation saved" };
        }
    }
}
