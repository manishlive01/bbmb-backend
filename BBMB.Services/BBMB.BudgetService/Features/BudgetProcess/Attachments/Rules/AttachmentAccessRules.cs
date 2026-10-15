using BBMB.BudgetService.Features.BudgetProcess.Attachments.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.Attachments.Rules
{
    /// <summary>
    /// Design §8.2 download row: "caller may view an owning record". An attachment is visible to the officer who
    /// uploaded it (an attachment not yet referenced has no other owner) and to every actor who can see a record
    /// referring to it, under the single scope rule of §4.9 (otherwise NOT_FOUND, never 403):
    /// T-26 AA copy (the approval's Division office), T-28 TS copy (the sanction's work's Division office), and
    /// the three T-30 demand-line attachments (the proposal's origin or holder office). Owning records added in
    /// later phases (T-34 upload batch, T-40 allocation minutes, T-46 re-appropriation order) extend this list.
    /// </summary>
    public sealed class AttachmentAccessRules(BudgetContext context, IBudgetOfficeScope officeScope)
    {
        public async Task<bool> CanViewAsync(BudgetAttachment attachment, BudgetActor actor, CancellationToken ct)
        {
            if (attachment.CreatedBy == actor.EmployeeId)
                return true;
            var id = attachment.AttachmentId;
            var visible = (await officeScope.GetVisibleOfficeIdsAsync(actor.OfficeId, ct)).ToList();

            var viaApproval = await context.AdminApprovalVersions.AsNoTracking()
                .Where(v => v.AaCopyAttachmentId == id)
                .Join(context.AdminApprovals, v => v.AdminApprovalId, a => a.AdminApprovalId, (v, a) => a.DivisionOfficeId)
                .AnyAsync(o => visible.Contains(o), ct);
            if (viaApproval)
                return true;

            var viaSanction = await context.TechnicalSanctionVersions.AsNoTracking()
                .Where(v => v.TsCopyAttachmentId == id)
                .Join(context.TechnicalSanctions, v => v.TechnicalSanctionId, t => t.TechnicalSanctionId, (v, t) => t.WorkId)
                .Join(context.Works, w => w, w => w.Id, (w, work) => work.DivisionId)
                .Join(context.BudgetOffices.Where(o => o.OfficeLevel == OfficeLevel.Division),
                    d => (int?)d, o => o.DivisionId, (d, o) => o.OfficeId)
                .AnyAsync(o => visible.Contains(o), ct);
            if (viaSanction)
                return true;

            return await context.DemandLines.AsNoTracking()
                .Where(l => l.JustificationAttachmentId == id || l.CeCertificateAttachmentId == id || l.CeAmrApprovalAttachmentId == id)
                .Join(context.BudgetProposals, l => l.ProposalId, p => p.ProposalId, (l, p) => p)
                .AnyAsync(p => visible.Contains(p.OriginOfficeId) || visible.Contains(p.HolderOfficeId), ct);
        }
    }
}
