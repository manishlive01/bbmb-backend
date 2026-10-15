using BBMB.BudgetService.Features.BudgetProcess.EstimateAllocations.GetForDivision.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Provisions.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.EstimateAllocations.GetForDivision
{
    /// <summary>
    /// Design §8.6 GET estimate-allocations (F-29, BUD-288, BUD-289). For the division and year, each work with
    /// a live operative WORK provision (V-02): its available amount, the estimate codes of its sanctions and
    /// what is allocated to each (T-43), the unallocated balance, and whether a critical correction flag stands
    /// (BUD-309). A read: nothing is written. Scope (DDO own Division, others view) is the endpoint policy
    /// Budget.View; the division is a query parameter.
    /// </summary>
    public sealed class GetForDivisionHandler(BudgetContext context)
        : IRequestHandler<GetForDivisionQuery, List<EstimateAllocationWorkDto>>
    {
        public async Task<List<EstimateAllocationWorkDto>> Handle(GetForDivisionQuery request, CancellationToken ct)
        {
            // Offices of the division (a provision's office is the Division of the work, DB-04).
            var officeIds = await context.BudgetOffices.AsNoTracking()
                .Where(o => o.DivisionId == request.DivisionId).Select(o => o.OfficeId).ToListAsync(ct);
            if (officeIds.Count == 0)
                return [];

            var provisions = await (
                from v in context.ProvisionBalances.AsNoTracking()
                where v.IsOperative && v.HolderType == ProvisionHolderType.Work && v.FinYear == request.FinYear
                      && officeIds.Contains(v.OfficeId) && v.WorkId != null
                join w in context.Works.AsNoTracking() on v.WorkId equals w.Id
                select new { v.ProvisionId, WorkId = v.WorkId!.Value, w.WorkCode, w.WorkDesc, v.FinYear, v.Available })
                .ToListAsync(ct);
            if (provisions.Count == 0)
                return [];

            var provisionIds = provisions.Select(p => p.ProvisionId).ToList();
            var workIds = provisions.Select(p => p.WorkId).Distinct().ToList();

            var allocations = await context.EstimateAllocations.AsNoTracking()
                .Where(e => provisionIds.Contains(e.ProvisionId))
                .Select(e => new { e.ProvisionId, e.TechnicalSanctionId, e.Amount }).ToListAsync(ct);

            var sanctions = await context.TechnicalSanctions.AsNoTracking()
                .Where(t => workIds.Contains(t.WorkId) && t.EstimateCode != null)
                .Select(t => new { t.TechnicalSanctionId, t.WorkId, EstimateCode = t.EstimateCode! }).ToListAsync(ct);

            var criticalWorks = (await context.CorrectionFlags.AsNoTracking()
                .Where(f => f.IsCritical && f.ClearedAt == null && f.WorkId != null && workIds.Contains(f.WorkId.Value))
                .Select(f => f.WorkId!.Value).ToListAsync(ct)).ToHashSet();

            return provisions.Select(p =>
            {
                var allocBySanction = allocations.Where(a => a.ProvisionId == p.ProvisionId)
                    .ToDictionary(a => a.TechnicalSanctionId, a => a.Amount);
                var estimates = sanctions.Where(s => s.WorkId == p.WorkId)
                    .Select(s => new EstimateAllocationCodeDto(
                        s.TechnicalSanctionId, s.EstimateCode, allocBySanction.GetValueOrDefault(s.TechnicalSanctionId)))
                    .OrderBy(e => e.EstimateCode).ToList();
                var allocated = allocBySanction.Values.Sum();
                return new EstimateAllocationWorkDto
                {
                    ProvisionId = p.ProvisionId,
                    WorkId = p.WorkId,
                    WorkCode = p.WorkCode,
                    WorkDesc = p.WorkDesc,
                    FinYear = p.FinYear,
                    Available = p.Available,
                    Allocated = allocated,
                    UnallocatedBalance = p.Available - allocated,
                    CriticalFlag = criticalWorks.Contains(p.WorkId),
                    Estimates = estimates,
                };
            }).OrderBy(r => r.WorkCode ?? string.Empty).ThenBy(r => r.WorkId).ToList();
        }
    }
}
