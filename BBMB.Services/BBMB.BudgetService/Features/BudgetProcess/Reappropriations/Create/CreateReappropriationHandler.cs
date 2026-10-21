using BBMB.BudgetService.Features.BudgetProcess.Provisions.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Create
{
    /// <summary>
    /// Design §8.7 POST reappropriations (F-21; BUD-201, BUD-205, BUD-282, BUD-283). Re-appropriation is the
    /// only in-year change to provision after the freeze (SRS §6). Order, nothing written on refusal:
    /// <list type="number">
    /// <item>actor (M-20); the year exists and is not closed (DB-22, M-17);</item>
    /// <item>exactly one destination (validator); an existing destination provision is a live provision; a
    /// new-work destination is a work with no code and no provision (BUD-282, BUD-283), else M-19;</item>
    /// <item>one OPEN entry per officer (BUD-201; pre-checked and the partial UK on created_by);</item>
    /// <item>the T-46 row at OPEN with the next serial for the year, in one REAPPROP_CREATE scope.</item>
    /// </list>
    /// Right F21.Enter (endpoint).
    /// </summary>
    public sealed class CreateReappropriationHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope)
        : IRequestHandler<CreateReappropriationCommand, Response<long>>
    {
        public async Task<Response<long>> Handle(CreateReappropriationCommand request, CancellationToken ct)
        {
            var dto = request.Dto;
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<long>(BudgetMessages.M20);

            // The year exists and is open (DB-22): no change against a closed year.
            var year = await context.FinancialYears.AsNoTracking()
                .Where(y => y.FinYearClosing == dto.FinYear).Select(y => new { y.FinYearClosing, y.Closed }).FirstOrDefaultAsync(ct);
            if (year is null)
                return BudgetResponses.Fail<long>(BudgetMessages.M05);
            if (year.Closed is not null)
                return BudgetResponses.Fail<long>(BudgetMessages.M17);

            // Destination: either an existing live provision, or a new work with no code and no provision.
            if (dto.DestinationProvisionId is { } destProv)
            {
                var live = await context.Provisions.AsNoTracking()
                    .AnyAsync(p => p.ProvisionId == destProv && p.SupersededByProvisionId == null, ct);
                if (!live)
                    return BudgetResponses.Fail<long>(BudgetMessages.M19);
            }
            else if (dto.DestinationNewWorkId is { } newWork)
            {
                var work = await context.Works.AsNoTracking()
                    .Where(w => w.Id == newWork).Select(w => new { w.Id, w.WorkCode }).FirstOrDefaultAsync(ct);
                if (work is null)
                    return BudgetResponses.Fail<long>(BudgetMessages.NotFound);
                if (work.WorkCode is not null)
                    return BudgetResponses.Fail<long>(BudgetMessages.M19); // a new-work destination has no code
                if (await context.Provisions.AsNoTracking().AnyAsync(p => p.WorkId == newWork, ct))
                    return BudgetResponses.Fail<long>(BudgetMessages.M19); // and no provision yet (BUD-283)
            }

            // One OPEN entry per officer (BUD-201); the partial UK on created_by is the hard guard.
            if (await context.Reappropriations.AsNoTracking()
                    .AnyAsync(r => r.Status == ReappropriationStatus.Open && r.CreatedBy == actor.EmployeeId, ct))
                return BudgetResponses.Fail<long>(BudgetMessages.M19);

            await using var tx = await scope.BeginAsync("REAPPROP_CREATE", ct: ct);
            // Serialise serial allotment per year (as the proposal/open-proposals pattern).
            await context.Database.ExecuteSqlRawAsync("LOCK TABLE budget.reappropriation IN SHARE ROW EXCLUSIVE MODE", ct);
            var nextSerial = (await context.Reappropriations.AsNoTracking()
                .Where(r => r.FinYear == dto.FinYear).MaxAsync(r => (int?)r.SerialNo, ct) ?? 0) + 1;

            var row = new Reappropriation
            {
                FinYear = dto.FinYear,
                SerialNo = nextSerial,
                Status = ReappropriationStatus.Open,
                DestinationProvisionId = dto.DestinationProvisionId,
                DestinationNewWorkId = dto.DestinationNewWorkId,
                AdditionalAmount = dto.AdditionalAmount,
                EofficeOrderRef = dto.EofficeOrderRef,
                Remarks = dto.Remarks,
            };
            context.Reappropriations.Add(row);
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new Response<long> { IsSucced = true, Message = "Re-appropriation created", Data = row.ReappropriationId };
        }
    }
}
