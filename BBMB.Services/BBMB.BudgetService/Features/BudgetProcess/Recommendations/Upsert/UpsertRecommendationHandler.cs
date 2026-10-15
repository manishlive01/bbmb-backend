using BBMB.BudgetService.Features.BudgetProcess.Cycles.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Features.BudgetProcess.Recommendations.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.Recommendations.Upsert
{
    /// <summary>
    /// Design §8.3 Upsert (F-13 columns 9 and 10; BUD-153, BUD-302, BUD-303). Order: actor (M-20) → cycle absent
    /// NOT_FOUND, not OPEN M-17 (the budget is frozen) → ceOfficeId an active CE office, else M-22 (DB-01) → the
    /// code exists and is in use (M-05, VR-09) → every proposal of the category whose origin is the CE or an
    /// office under it is at S-10 or S-11, and at least one exists, else M-19 → one T-37 row per (cycle, category,
    /// CE, code): inserted or updated under action RECOMMENDATION; each change writes the next T-38 version by
    /// trigger (DB-14). From P4, after an allocation exists, <c>minutesAllocationId</c> is also required (DB-17).
    /// Right F13.Act (endpoint).
    /// </summary>
    public sealed class UpsertRecommendationHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope)
        : IRequestHandler<UpsertRecommendationCommand, Response<long>>
    {
        public const string ActionCode = "RECOMMENDATION";
        private static readonly string[] Finalisable = [ProposalStateCode.S10, ProposalStateCode.S11];

        public async Task<Response<long>> Handle(UpsertRecommendationCommand request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail<long>(BudgetMessages.M20);
            var dto = request.Dto;
            var cycleStatus = await context.BudgetCycles.AsNoTracking()
                .Where(c => c.CycleId == request.CycleId).Select(c => c.Status).FirstOrDefaultAsync(ct);
            if (cycleStatus is null)
                return BudgetResponses.Fail<long>(BudgetMessages.NotFound);
            if (cycleStatus != CycleStatus.Open)
                return BudgetResponses.Fail<long>(BudgetMessages.M17);
            if (!await context.BudgetOffices.AsNoTracking()
                    .AnyAsync(o => o.OfficeId == dto.CeOfficeId && o.IsActive && o.OfficeLevel == OfficeLevel.Ce, ct))
                return BudgetResponses.Fail<long>(BudgetMessages.M22);
            if (!await context.HeadOfAccounts.AsNoTracking().AnyAsync(h => h.AccountCodeId == dto.AccountCodeId && !h.NotForUse, ct))
                return BudgetResponses.Fail<long>(BudgetMessages.M05);

            var circles = context.BudgetOffices.AsNoTracking()
                .Where(o => o.ParentOfficeId == dto.CeOfficeId && o.OfficeLevel == OfficeLevel.Circle).Select(o => o.OfficeId);
            var underCe = context.BudgetOffices.AsNoTracking()
                .Where(o => o.OfficeId == dto.CeOfficeId || circles.Contains(o.OfficeId) ||
                            (o.OfficeLevel == OfficeLevel.Division && o.ParentOfficeId != null && circles.Contains(o.ParentOfficeId.Value)))
                .Select(o => o.OfficeId);
            var states = await context.BudgetProposals.AsNoTracking()
                .Where(p => p.CycleId == request.CycleId && p.Category == dto.Category && underCe.Contains(p.OriginOfficeId))
                .Select(p => p.StateCode).ToListAsync(ct);
            if (states.Count == 0 || states.Any(s => !Finalisable.Contains(s)))
                return BudgetResponses.Fail<long>(BudgetMessages.M19);

            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            await context.Database.ExecuteSqlRawAsync("LOCK TABLE budget.ce_recommendation IN SHARE ROW EXCLUSIVE MODE", ct);
            var row = await context.CeRecommendations.FirstOrDefaultAsync(r =>
                r.CycleId == request.CycleId && r.Category == dto.Category && r.CeOfficeId == dto.CeOfficeId &&
                r.AccountCodeId == dto.AccountCodeId, ct);
            if (row is null)
            {
                row = new CeRecommendation
                {
                    CycleId = request.CycleId, Category = dto.Category, CeOfficeId = dto.CeOfficeId, AccountCodeId = dto.AccountCodeId,
                };
                context.CeRecommendations.Add(row);
            }
            row.RbeRecommended = dto.RbeRecommended;
            row.ObeNextRecommended = dto.ObeNextRecommended;
            row.ConcurrenceRef = string.IsNullOrWhiteSpace(dto.ConcurrenceRef) ? null : dto.ConcurrenceRef.Trim();
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new Response<long> { IsSucced = true, Message = "Recommendation recorded", Data = row.RecommendationId };
        }
    }
}
