using BBMB.BudgetService.Features.BudgetProcess.CorrectionFlags.GetList.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.CorrectionFlags.GetList
{
    /// <summary>
    /// Design §8.6 correction-flags (BUD-309). Finance and the Secretariat see every flag; another office sees
    /// only its own — flags on a work of a Division it can see, or on a provision held at an office it can see.
    /// Each flag carries the allocated and supported figures, their difference, and the M-34 text. When
    /// <c>cycleId</c> is given, the list is restricted to flags on a provision allocated in that cycle. A read:
    /// nothing is written. Right Budget.View (endpoint).
    /// </summary>
    public sealed class GetCorrectionFlagsHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope)
        : IRequestHandler<GetCorrectionFlagsQuery, List<CorrectionFlagDto>>
    {
        public async Task<List<CorrectionFlagDto>> Handle(GetCorrectionFlagsQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return [];

            var query = context.CorrectionFlags.AsNoTracking().AsQueryable();
            if (request.Open)
                query = query.Where(f => f.ClearedAt == null);

            // Scope: Finance / Secretariat see all; others only their own (works of visible Divisions, or
            // provisions held at a visible office).
            var seesAll = actor.OfficeLevel is OfficeLevel.Finance or OfficeLevel.Secretariat;
            if (!seesAll)
            {
                var visible = await officeScope.GetVisibleOfficeIdsAsync(actor.OfficeId, ct);
                var visibleDivisionIds = await context.BudgetOffices.AsNoTracking()
                    .Where(o => visible.Contains(o.OfficeId) && o.OfficeLevel == OfficeLevel.Division && o.DivisionId != null)
                    .Select(o => o.DivisionId!.Value).ToListAsync(ct);

                var ownWorkIds = await context.Works.AsNoTracking()
                    .Where(w => visibleDivisionIds.Contains(w.DivisionId)).Select(w => w.Id).ToListAsync(ct);
                var ownProvisionIds = await context.Provisions.AsNoTracking()
                    .Where(p => visible.Contains(p.OfficeId)).Select(p => p.ProvisionId).ToListAsync(ct);

                query = query.Where(f =>
                    (f.WorkId != null && ownWorkIds.Contains(f.WorkId.Value)) ||
                    (f.ProvisionId != null && ownProvisionIds.Contains(f.ProvisionId.Value)));
            }

            if (request.CycleId is { } cycleId)
            {
                var cycleProvisionIds = await context.Provisions.AsNoTracking()
                    .Where(p => p.AllocationId != null &&
                                context.Allocations.Any(a => a.AllocationId == p.AllocationId && a.CycleId == cycleId))
                    .Select(p => p.ProvisionId).ToListAsync(ct);
                query = query.Where(f => f.ProvisionId != null && cycleProvisionIds.Contains(f.ProvisionId.Value));
            }

            var flags = await query.OrderBy(f => f.CorrectionFlagId).ToListAsync(ct);
            return flags.Select(f => new CorrectionFlagDto
            {
                CorrectionFlagId = f.CorrectionFlagId,
                Reason = f.Reason,
                WorkId = f.WorkId,
                ProvisionId = f.ProvisionId,
                FigureAllocated = f.FigureAllocated,
                FigureSupported = f.FigureSupported,
                Difference = f.FigureAllocated is { } a && f.FigureSupported is { } s ? a - s : null,
                IsCritical = f.IsCritical,
                Cleared = f.ClearedAt != null,
                Message = BudgetMessages.Format(BudgetMessages.M34),
            }).ToList();
        }
    }
}
