using BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Entities;
using BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Revise
{
    /// <summary>
    /// F-06 revision (design §8.4; BUD-271 to BUD-273). Order: actor (M-20) → sanction absent or its work's Division
    /// office not visible → NOT_FOUND → actor not the DDO of that Division → M-20 → copy (M-07) → TS number unused by
    /// another sanction of the Division (DB-07, M-19) → VR-11 (M-31; not applied and stated while Financial Accounting
    /// is unavailable, OI-15) → TS_REVISE scope with the header row lock: VR-02 for the AA tab with this sanction's
    /// current version replaced by the new amount (M-06) → T-28 version current + 1 and the header moved (DB-06).
    /// The estimate code, tab, work and approval do not change.
    /// </summary>
    public sealed class ReviseTechnicalSanctionHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, IBudgetOfficeScope officeScope,
        TechnicalSanctionRules rules)
        : IRequestHandler<ReviseTechnicalSanctionCommand, Response<int>>
    {
        public const string ActionCode = "TS_REVISE";

        public async Task<Response<int>> Handle(ReviseTechnicalSanctionCommand request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<int>(BudgetMessages.M20);
            var found = await context.TechnicalSanctions.AsNoTracking()
                .Where(t => t.TechnicalSanctionId == request.TechnicalSanctionId)
                .Join(context.Works, t => t.WorkId, w => w.Id, (t, w) => new { Sanction = t, w.DivisionId })
                .FirstOrDefaultAsync(ct);
            var officeId = found is null ? null : await context.BudgetOffices.AsNoTracking()
                .Where(o => o.DivisionId == found.DivisionId && o.OfficeLevel == Offices.Entities.OfficeLevel.Division)
                .Select(o => (long?)o.OfficeId).FirstOrDefaultAsync(ct);
            if (found is null || officeId is null ||
                !(await officeScope.GetVisibleOfficeIdsAsync(actor.OfficeId, ct)).Contains(officeId.Value))
                return BudgetResponses.Fail<int>(BudgetMessages.NotFound);
            if (await rules.ActorDivisionAsync(actor, ct) is not { } division || division.DivisionId != found.DivisionId)
                return BudgetResponses.Fail<int>(BudgetMessages.M20);
            var dto = request.Dto;
            if (!await rules.IsUsableEvidenceAsync(dto.TsCopyAttachmentId!.Value, actor, ct))
                return BudgetResponses.Fail<int>(BudgetMessages.M07);
            var tsNumber = dto.TsNumber.Trim();
            if (await rules.IsTsNumberUsedAsync(found.DivisionId, tsNumber, found.Sanction.TechnicalSanctionId, ct))
                return BudgetResponses.Fail<int>(BudgetMessages.M19);
            var (booked, notApplied) = await rules.CheckBookedAsync(found.Sanction.EstimateCode, dto.TsAmount, ct);
            if (booked is not null)
                return BudgetResponses.Fail<int>(booked);

            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT 1 FROM budget.technical_sanction WHERE technical_sanction_id = {found.Sanction.TechnicalSanctionId} FOR UPDATE", ct);
            var header = await context.TechnicalSanctions.SingleAsync(t => t.TechnicalSanctionId == found.Sanction.TechnicalSanctionId, ct);
            if (header.Tab == SanctionTab.Aa &&
                await rules.CheckAaCeilingAsync(header.AdminApprovalId!.Value, dto.TsAmount, header.TechnicalSanctionId, ct) is { } ceiling)
                return BudgetResponses.Fail<int>(ceiling);
            var next = (short)(header.CurrentVersionNo + 1);
            context.TechnicalSanctionVersions.Add(new TechnicalSanctionVersion
            {
                TechnicalSanctionId = header.TechnicalSanctionId, VersionNo = next, NameOfEstimate = dto.NameOfEstimate.Trim(),
                TsNumber = tsNumber, TsDate = dto.TsDate, TsAmount = dto.TsAmount, TsCopyAttachmentId = dto.TsCopyAttachmentId.Value,
            });
            header.CurrentVersionNo = next;
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new Response<int>
            {
                IsSucced = true,
                Message = notApplied is null ? $"Sanction revised to version {next}" : $"Sanction revised to version {next}. {notApplied}",
                Data = next,
            };
        }
    }
}
