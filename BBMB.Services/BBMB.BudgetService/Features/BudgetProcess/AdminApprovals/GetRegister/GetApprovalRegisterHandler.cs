using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.GetRegister.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.GetRegister
{
    /// <summary>
    /// Design §8.8 (F-05, BUD-066, BUD-067). Order: actor (M-20). An approval is held at a Division office
    /// (DB-04). The register is scoped to the Division offices in the actor's visible set; where
    /// <c>officeId</c> is given, it is further narrowed to the Divisions under that office (itself a Division,
    /// or the Divisions of a Circle, or of a CE), so a CE or Circle user reads its subtree (BUD-067). The
    /// <c>level</c> hint names how <c>officeId</c> is to be read; the subtree is resolved from the hierarchy
    /// regardless. Each approval carries every version (BUD-066), newest first.
    /// </summary>
    public sealed class GetApprovalRegisterHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope)
        : IRequestHandler<GetApprovalRegisterQuery, Response<List<ApprovalRegisterRowDto>>>
    {
        public async Task<Response<List<ApprovalRegisterRowDto>>> Handle(GetApprovalRegisterQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<List<ApprovalRegisterRowDto>>(BudgetMessages.M20);

            var visible = await officeScope.GetVisibleOfficeIdsAsync(actor.OfficeId, ct);
            IReadOnlySet<long> scopeOffices = visible;
            if (request.OfficeId is { } officeId)
            {
                var subtree = await DivisionOfficesUnderAsync(officeId, ct);
                scopeOffices = subtree.Where(visible.Contains).ToHashSet();
            }

            var approvals = await context.AdminApprovals.AsNoTracking()
                .Where(a => scopeOffices.Contains(a.DivisionOfficeId))
                .Select(a => new { a.AdminApprovalId, a.WorkId, a.DivisionOfficeId, a.SerialNo, a.CurrentVersionNo })
                .ToListAsync(ct);
            var ids = approvals.Select(a => a.AdminApprovalId).ToList();
            var versions = (await context.AdminApprovalVersions.AsNoTracking()
                    .Where(v => ids.Contains(v.AdminApprovalId))
                    .Select(v => new { v.AdminApprovalId, v.VersionNo, v.NameOfWork, v.AaReferenceNo, v.DateOfApproval,
                        v.AccountCodeId, v.AmountSanctioned, v.CompetentAuthority })
                    .ToListAsync(ct))
                .GroupBy(v => v.AdminApprovalId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(v => v.VersionNo)
                    .Select(v => new ApprovalVersionDto(v.VersionNo, v.NameOfWork, v.AaReferenceNo, v.DateOfApproval,
                        v.AccountCodeId, v.AmountSanctioned, v.CompetentAuthority)).ToList());

            var data = approvals
                .OrderBy(a => a.DivisionOfficeId).ThenBy(a => a.SerialNo)
                .Select(a => new ApprovalRegisterRowDto(
                    a.AdminApprovalId, a.WorkId, a.DivisionOfficeId, a.SerialNo, a.CurrentVersionNo,
                    versions.GetValueOrDefault(a.AdminApprovalId, [])))
                .ToList();

            return new Response<List<ApprovalRegisterRowDto>> { IsSucced = true, Message = "Approval register", Data = data };
        }

        /// <summary>The Division office ids at or under <paramref name="officeId"/>: itself if a Division, the
        /// Divisions of a Circle, or every Division of a CE's Circles.</summary>
        private async Task<HashSet<long>> DivisionOfficesUnderAsync(long officeId, CancellationToken ct)
        {
            var office = await context.BudgetOffices.AsNoTracking()
                .Where(o => o.OfficeId == officeId).Select(o => o.OfficeLevel).FirstOrDefaultAsync(ct);
            if (office == OfficeLevel.Division)
                return [officeId];
            if (office == OfficeLevel.Circle)
                return (await context.BudgetOffices.AsNoTracking()
                    .Where(o => o.OfficeLevel == OfficeLevel.Division && o.ParentOfficeId == officeId)
                    .Select(o => o.OfficeId).ToListAsync(ct)).ToHashSet();
            if (office == OfficeLevel.Ce)
            {
                var circles = await context.BudgetOffices.AsNoTracking()
                    .Where(o => o.OfficeLevel == OfficeLevel.Circle && o.ParentOfficeId == officeId)
                    .Select(o => o.OfficeId).ToListAsync(ct);
                return (await context.BudgetOffices.AsNoTracking()
                    .Where(o => o.OfficeLevel == OfficeLevel.Division && o.ParentOfficeId != null && circles.Contains(o.ParentOfficeId.Value))
                    .Select(o => o.OfficeId).ToListAsync(ct)).ToHashSet();
            }
            return [];
        }
    }
}
