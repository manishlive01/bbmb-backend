using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Features.Works.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Works.SetLegacyCode
{
    /// <summary>
    /// Design §8.6 (F-28, BUD-306, L-08). Keys a continuing work's legacy code in the first cycle only:
    /// the work carries no code, the first-cycle guard holds, and the code is written once with
    /// <c>work_code_origin = LEGACY</c>, <c>work_code_occasion = LEGACY_OPENING</c> and <c>work_code_fin_year</c>
    /// null. Write-once is also enforced in the database (DB-05); the handler's own "work has no code" check
    /// gives the catalogue message M-19 instead of a raw constraint failure.
    /// </summary>
    public sealed class SetLegacyCodeHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, WorkRules rules)
        : IRequestHandler<SetLegacyCodeCommand, Response>
    {
        public async Task<Response> Handle(SetLegacyCodeCommand request, CancellationToken ct)
        {
            // work carries fn_require_actor (DB-28): every write runs inside the action scope.
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail(BudgetMessages.M20);

            // Division: only a work of the actor's Division is addressed; any other is 404 (design §4.9), so a
            // work of another Division is never disclosed.
            var actorDivisionId = await context.BudgetOffices.AsNoTracking()
                .Where(o => o.OfficeId == actor.OfficeId && o.OfficeLevel == OfficeLevel.Division)
                .Select(o => o.DivisionId)
                .FirstOrDefaultAsync(ct);
            var work = actorDivisionId is null
                ? null
                : await context.Works.FirstOrDefaultAsync(w => w.Id == request.Id && w.DivisionId == actorDivisionId, ct);
            if (work is null)
                return BudgetResponses.Fail(BudgetMessages.NotFound);

            // Write-once (DB-05): a work that already holds a code (generated or legacy) is refused with M-19
            // rather than left to the database trigger, so the response carries the catalogue message.
            if (!string.IsNullOrEmpty(work.WorkCode))
                return BudgetResponses.Fail(BudgetMessages.M19);

            // First-cycle guard (design §5.3), the same WorkRules.CheckFirstCycleAsync as POST /api/works and
            // PUT /api/works/{id} clearing pre_ddd. A continuing work's legacy code is keyed only in the first
            // cycle (L-08).
            if (await rules.CheckFirstCycleAsync(actor.OfficeId, ct) is { } guard)
                return BudgetResponses.Fail(guard);

            work.WorkCode = request.dto.LegacyCode;
            work.WorkCodeOrigin = WorkCodeOrigin.Legacy;
            work.WorkCodeOccasion = WorkCodeOccasion.LegacyOpening;
            work.WorkCodeFinYear = null;
            work.WorkCodeAt = DateTimeOffset.UtcNow;

            await using var tx = await scope.BeginAsync("WORKS_LEGACY_CODE", ct: ct);
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return new Response { IsSucced = true, Message = "Legacy work code recorded successfully." };
        }
    }
}
