using BBMB.BudgetService.Features.BudgetProcess.Attachments.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Cycles;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Settings;
using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Rules
{
    /// <summary>
    /// Application rules of F-03 / F-04 (design §8.4 admin-approval rows; DDD §6.2): BUD-055 delegation warning,
    /// the DB-07 pre-check, the serial allotted in the Division (BUD-053), the evidence check (VR-10, M-04) and the
    /// BUD-059 work line. The database keeps DB-04 to DB-08 as backstops.
    /// </summary>
    public sealed class AdminApprovalRules(BudgetContext context, IDelegationLimitReader delegation)
    {
        /// <summary>
        /// BUD-055: true when the T-19 limit of <paramref name="authority"/> in force on the date of approval (the
        /// date the power was exercised) is set and below <paramref name="amount"/>. No row, or a null amount, is no
        /// limit. The check warns and never refuses: the caller asks for a justification (M-27).
        /// </summary>
        public async Task<bool> CheckDelegationAsync(string authority, decimal amount, DateOnly dateOfApproval, CancellationToken ct = default)
        {
            var limit = await delegation.GetInForceAsync(authority, dateOfApproval, ct);
            return limit?.MaxAmount is { } max && amount > max;
        }

        /// <summary>The DIVISION office of the actor, with its master Division, or null when the actor is not at a Division.</summary>
        public async Task<(long OfficeId, int DivisionId)?> ActorDivisionAsync(BudgetActor actor, CancellationToken ct = default)
        {
            if (actor.OfficeLevel != OfficeLevel.Division)
                return null;
            var division = await context.BudgetOffices.AsNoTracking()
                .Where(o => o.OfficeId == actor.OfficeId && o.OfficeLevel == OfficeLevel.Division)
                .Select(o => o.DivisionId).FirstOrDefaultAsync(ct);
            return division is { } d ? (actor.OfficeId, d) : null;
        }

        /// <summary>
        /// VR-10 (BUD-057, M-04): the copy exists, is of evidence class (DB-08) and was uploaded by the actor (an
        /// officer cannot attach another officer's unreferenced upload).
        /// </summary>
        public Task<bool> IsUsableEvidenceAsync(long attachmentId, BudgetActor actor, CancellationToken ct = default) =>
            context.BudgetAttachments.AsNoTracking().AnyAsync(a =>
                a.AttachmentId == attachmentId && a.EvidenceClass == AttachmentEvidenceClass.Evidence &&
                a.CreatedBy == actor.EmployeeId, ct);

        /// <summary>
        /// DB-07 pre-check: the reference is used by another approval of the Division. An approval's own earlier
        /// versions may repeat it (<paramref name="ownApprovalId"/>).
        /// </summary>
        public Task<bool> IsReferenceUsedAsync(long divisionOfficeId, string reference, long? ownApprovalId, CancellationToken ct = default) =>
            context.AdminApprovalVersions.AsNoTracking()
                .Where(v => v.AaReferenceNo == reference)
                .Join(context.AdminApprovals, v => v.AdminApprovalId, a => a.AdminApprovalId, (v, a) => a)
                .AnyAsync(a => a.DivisionOfficeId == divisionOfficeId && a.AdminApprovalId != ownApprovalId, ct);

        /// <summary>
        /// BUD-053: the next serial in the Division, under a transaction advisory lock so concurrent approvals of one
        /// Division do not collide on the UK (the UK maps to M-19 as the backstop). Call inside the action scope.
        /// </summary>
        public async Task<int> NextSerialAsync(long divisionOfficeId, CancellationToken ct = default)
        {
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock(hashtext('budget.aa_serial'), {(int)(divisionOfficeId % int.MaxValue)})", ct);
            return (await context.AdminApprovals.AsNoTracking()
                .Where(a => a.DivisionOfficeId == divisionOfficeId).MaxAsync(a => (int?)a.SerialNo, ct) ?? 0) + 1;
        }

        /// <summary>
        /// BUD-059: when the Division's WORKS proposal of the current cycle (§5.3) is at S-01 or S-04, the work gets a
        /// WORK line with null figures (the DDO enters them at F-07), unless it already has a live line there. No
        /// current cycle, no proposal, or a proposal in another state: no line, and the approval stands.
        /// Returns the new line id, or null. Call inside the action scope after the work is saved.
        /// </summary>
        public async Task<long?> AddWorkLineIfOpenAsync(long divisionOfficeId, Work work, long accountCodeId, CancellationToken ct = default)
        {
            var cycle = await BudgetCycleQueries.GetCurrentAsync(context, ct);
            if (cycle is null)
                return null;
            var p = await context.BudgetProposals.AsNoTracking().FirstOrDefaultAsync(x =>
                x.CycleId == cycle.CycleId && x.Category == ProposalCategory.Works && x.OriginOfficeId == divisionOfficeId, ct);
            if (p is null || (p.StateCode != ProposalStateCode.S01 && p.StateCode != ProposalStateCode.S04))
                return null;
            await DemandLineWrites.LockProposalAsync(context, p.ProposalId, ct);
            if (await context.DemandLines.AsNoTracking().AnyAsync(l => l.ProposalId == p.ProposalId && l.WorkId == work.Id && !l.IsDeleted, ct))
                return null;
            var line = new DemandLine
            {
                ProposalId = p.ProposalId, LineType = DemandLineType.Work, WorkId = work.Id, AccountCodeId = accountCodeId,
                AddedOnReturn = p.StateCode == ProposalStateCode.S04,
                SerialNo = (await context.DemandLines.AsNoTracking()
                    .Where(l => l.ProposalId == p.ProposalId).MaxAsync(l => (int?)l.SerialNo, ct) ?? 0) + 1,
            };
            context.DemandLines.Add(line);
            await context.SaveChangesAsync(ct);
            return line.DemandLineId;
        }
    }
}
