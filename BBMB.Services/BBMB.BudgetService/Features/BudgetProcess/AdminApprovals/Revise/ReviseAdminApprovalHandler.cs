using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Entities;
using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Rules;
using BBMB.BudgetService.Features.BudgetProcess.CorrectionFlags.Entities;
using BBMB.BudgetService.Features.Works.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Revise
{
    /// <summary>
    /// F-04 (design §8.4 revisions row; BUD-063 to BUD-065, BUD-269, BUD-074). Order: actor (M-20) → approval absent
    /// or its Division office not visible → NOT_FOUND (§4.9) → actor is not the DDO of that Division → M-20 →
    /// code (M-05) → copy (M-04) → reference unused by another approval of the Division (DB-07, M-19; its own earlier
    /// versions may repeat it) → BUD-055 (M-27 without justification). Then, in one AA_REVISE scope holding the
    /// header row lock: T-26 version <c>current + 1</c>, header <c>current_version_no</c> moved to it (DB-06), and
    /// the T-45 flags. Figures are never altered by a revision:
    /// <list type="bullet">
    /// <item><c>AA_REDUCED</c> when the new amount is below the work's demanded sum (rbe + obe of its live demand
    /// lines in the current cycle; BUD-065). The P4 "or a live provision" arm is added with T-41.</item>
    /// <item><c>AA_HEAD_CHANGED</c> when the account code changes and a live demand line of the work exists
    /// (BUD-269); the provision arm is added in P4.</item>
    /// </list>
    /// An upward revision re-admits sanctions without any action here: DB-09 reads the current version (BUD-074).
    /// </summary>
    public sealed class ReviseAdminApprovalHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, IBudgetOfficeScope officeScope,
        AdminApprovalRules rules, WorkRules workRules)
        : IRequestHandler<ReviseAdminApprovalCommand, Response<int>>
    {
        public const string ActionCode = "AA_REVISE";

        public async Task<Response<int>> Handle(ReviseAdminApprovalCommand request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<int>(BudgetMessages.M20);
            var approval = await context.AdminApprovals.AsNoTracking()
                .FirstOrDefaultAsync(a => a.AdminApprovalId == request.AdminApprovalId, ct);
            if (approval is null || !(await officeScope.GetVisibleOfficeIdsAsync(actor.OfficeId, ct)).Contains(approval.DivisionOfficeId))
                return BudgetResponses.Fail<int>(BudgetMessages.NotFound);
            if (await rules.ActorDivisionAsync(actor, ct) is not { } division || division.OfficeId != approval.DivisionOfficeId)
                return BudgetResponses.Fail<int>(BudgetMessages.M20);
            var dto = request.Dto;
            var (code, codeFailure) = await workRules.ResolveAccountCodeAsync(dto.AccountCodeId, ct);
            if (code is null)
                return BudgetResponses.Fail<int>(codeFailure!);
            if (!await rules.IsUsableEvidenceAsync(dto.AaCopyAttachmentId!.Value, actor, ct))
                return BudgetResponses.Fail<int>(BudgetMessages.M04);
            var reference = dto.AaReferenceNo.Trim();
            if (await rules.IsReferenceUsedAsync(approval.DivisionOfficeId, reference, approval.AdminApprovalId, ct))
                return BudgetResponses.Fail<int>(BudgetMessages.M19);
            var exceeds = await rules.CheckDelegationAsync(dto.CompetentAuthority, dto.AmountSanctioned, dto.DateOfApproval, ct);
            var justification = string.IsNullOrWhiteSpace(dto.DelegationJustification) ? null : dto.DelegationJustification.Trim();
            if (exceeds && justification is null)
                return BudgetResponses.Fail<int>(BudgetMessages.M27);

            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT 1 FROM budget.admin_approval WHERE admin_approval_id = {approval.AdminApprovalId} FOR UPDATE", ct);
            var header = await context.AdminApprovals.SingleAsync(a => a.AdminApprovalId == approval.AdminApprovalId, ct);
            var current = await context.AdminApprovalVersions.AsNoTracking()
                .SingleAsync(v => v.AdminApprovalId == header.AdminApprovalId && v.VersionNo == header.CurrentVersionNo, ct);
            var next = (short)(header.CurrentVersionNo + 1);
            context.AdminApprovalVersions.Add(new AdminApprovalVersion
            {
                AdminApprovalId = header.AdminApprovalId, VersionNo = next, NameOfWork = dto.NameOfWork.Trim(),
                AaReferenceNo = reference, DateOfApproval = dto.DateOfApproval, AccountCodeId = code.AccountCodeId,
                AmountSanctioned = dto.AmountSanctioned, AaCopyAttachmentId = dto.AaCopyAttachmentId.Value,
                EofficeRef = string.IsNullOrWhiteSpace(dto.EofficeRef) ? null : dto.EofficeRef.Trim(),
                CompetentAuthority = dto.CompetentAuthority, ExceedsDelegation = exceeds,
                DelegationJustification = exceeds ? justification : null,
            });
            header.CurrentVersionNo = next;

            var demand = await rules.DemandOfWorkAsync(header.WorkId, ct);
            if (demand.HasLiveLine && dto.AmountSanctioned < demand.Sum)
                context.CorrectionFlags.Add(new CorrectionFlag
                {
                    Reason = CorrectionFlagReason.AaReduced, WorkId = header.WorkId,
                    FigureAllocated = demand.Sum, FigureSupported = dto.AmountSanctioned,
                });
            if (demand.HasLiveLine && code.AccountCodeId != current.AccountCodeId)
                context.CorrectionFlags.Add(new CorrectionFlag { Reason = CorrectionFlagReason.AaHeadChanged, WorkId = header.WorkId });

            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new Response<int> { IsSucced = true, Message = $"Approval revised to version {next}", Data = next };
        }
    }
}
