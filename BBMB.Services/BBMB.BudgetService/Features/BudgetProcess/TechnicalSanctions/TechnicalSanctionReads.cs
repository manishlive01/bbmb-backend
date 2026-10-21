using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions
{
    /// <summary>Sanction reads limited to works of the visible Division offices (§4.9, BUD-031).</summary>
    public static class TechnicalSanctionReads
    {
        public static async Task<List<TechnicalSanctionDto>> LoadAsync(
            BudgetContext context, IReadOnlyCollection<long> visibleOfficeIds, long? sanctionId, int? workId, CancellationToken ct)
        {
            var visible = visibleOfficeIds.ToList();
            var divisions = await context.BudgetOffices.AsNoTracking()
                .Where(o => visible.Contains(o.OfficeId) && o.OfficeLevel == OfficeLevel.Division && o.DivisionId != null)
                .Select(o => o.DivisionId!.Value).ToListAsync(ct);
            var q = context.TechnicalSanctions.AsNoTracking()
                .Join(context.Works, t => t.WorkId, w => w.Id, (t, w) => new { t, w.DivisionId })
                .Where(x => divisions.Contains(x.DivisionId)).Select(x => x.t);
            if (sanctionId is { } id) q = q.Where(t => t.TechnicalSanctionId == id);
            if (workId is { } wid) q = q.Where(t => t.WorkId == wid);
            var headers = await q.OrderBy(t => t.WorkId).ThenBy(t => t.SerialNo).ToListAsync(ct);
            var ids = headers.Select(h => h.TechnicalSanctionId).ToList();
            var versions = (await context.TechnicalSanctionVersions.AsNoTracking()
                    .Where(v => ids.Contains(v.TechnicalSanctionId))
                    .OrderBy(v => v.VersionNo).ToListAsync(ct))
                .ToLookup(v => v.TechnicalSanctionId);
            return headers.Select(h => new TechnicalSanctionDto
            {
                TechnicalSanctionId = h.TechnicalSanctionId, WorkId = h.WorkId, Tab = h.Tab, AdminApprovalId = h.AdminApprovalId,
                SerialNo = h.SerialNo, CurrentVersionNo = h.CurrentVersionNo, EstimateCode = h.EstimateCode, EstimateCodeAt = h.EstimateCodeAt,
                Versions = versions[h.TechnicalSanctionId].Select(v => new TechnicalSanctionVersionDto
                {
                    VersionNo = v.VersionNo, NameOfEstimate = v.NameOfEstimate, TsNumber = v.TsNumber, TsDate = v.TsDate,
                    TsAmount = v.TsAmount, TsCopyAttachmentId = v.TsCopyAttachmentId, RecordedAt = v.CreatedAt,
                }).ToList(),
            }).ToList();
        }
    }
}
