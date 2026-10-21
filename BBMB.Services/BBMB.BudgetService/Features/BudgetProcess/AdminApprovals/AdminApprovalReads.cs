using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.AdminApprovals
{
    /// <summary>Approval reads limited to the approvals of the visible Division offices (BUD-067, §4.9).</summary>
    public static class AdminApprovalReads
    {
        public static async Task<List<AdminApprovalDto>> LoadAsync(
            BudgetContext context, IReadOnlyCollection<long> visibleOfficeIds, long? approvalId, int? workId, CancellationToken ct)
        {
            var visible = visibleOfficeIds.ToList();
            var q = context.AdminApprovals.AsNoTracking().Where(a => visible.Contains(a.DivisionOfficeId));
            if (approvalId is { } id) q = q.Where(a => a.AdminApprovalId == id);
            if (workId is { } w) q = q.Where(a => a.WorkId == w);
            var headers = await q.OrderBy(a => a.DivisionOfficeId).ThenBy(a => a.SerialNo).ToListAsync(ct);
            var ids = headers.Select(h => h.AdminApprovalId).ToList();
            var versions = await context.AdminApprovalVersions.AsNoTracking()
                .Where(v => ids.Contains(v.AdminApprovalId))
                .OrderBy(v => v.AdminApprovalId).ThenBy(v => v.VersionNo)
                .ToListAsync(ct);
            var byApproval = versions.ToLookup(v => v.AdminApprovalId);
            return headers.Select(h => new AdminApprovalDto
            {
                AdminApprovalId = h.AdminApprovalId, WorkId = h.WorkId, DivisionOfficeId = h.DivisionOfficeId,
                SerialNo = h.SerialNo, CurrentVersionNo = h.CurrentVersionNo,
                Versions = byApproval[h.AdminApprovalId].Select(v => new AdminApprovalVersionDto
                {
                    VersionNo = v.VersionNo, NameOfWork = v.NameOfWork, AaReferenceNo = v.AaReferenceNo,
                    DateOfApproval = v.DateOfApproval, AccountCodeId = v.AccountCodeId, AmountSanctioned = v.AmountSanctioned,
                    AaCopyAttachmentId = v.AaCopyAttachmentId, EofficeRef = v.EofficeRef, CompetentAuthority = v.CompetentAuthority,
                    ExceedsDelegation = v.ExceedsDelegation, DelegationJustification = v.DelegationJustification, RecordedAt = v.CreatedAt,
                }).ToList(),
            }).ToList();
        }
    }
}
