using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Provisions.Entities;
using BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Create
{
    /// <summary>
    /// Design §8.6 POST unforeseen-requests (F-30, DB-25; BUD-294, BUD-297). Order, nothing written on refusal:
    /// <list type="number">
    /// <item>actor (M-20); the actor's Division office (M-20);</item>
    /// <item>the work has a recorded approval and no work code (DB-25; pre-checked, also the DB trigger);</item>
    /// <item>at least one (provision, amount) pair; each named provision is an UNFORESEEN provision of the
    /// Division's office (BUD-297), else M-19;</item>
    /// <item>the T-48 row at OPEN, held by the Division office, in one UNFORESEEN_CREATE scope.</item>
    /// </list>
    /// Right F30.Submit (endpoint).
    /// </summary>
    public sealed class CreateUnforeseenRequestHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope)
        : IRequestHandler<CreateUnforeseenRequestCommand, Response<long>>
    {
        public async Task<Response<long>> Handle(CreateUnforeseenRequestCommand request, CancellationToken ct)
        {
            var dto = request.Dto;
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<long>(BudgetMessages.M20);

            var divisionOffice = await context.BudgetOffices.AsNoTracking()
                .Where(o => o.OfficeId == actor.OfficeId && o.OfficeLevel == OfficeLevel.Division)
                .Select(o => new { o.OfficeId, o.DivisionId }).FirstOrDefaultAsync(ct);
            if (divisionOffice is null)
                return BudgetResponses.Fail<long>(BudgetMessages.M20);

            // DB-25: the work carries a recorded approval and no work code (also enforced by the DB trigger).
            var work = await context.Works.AsNoTracking()
                .Where(w => w.Id == dto.WorkId).Select(w => new { w.Id, w.WorkCode, w.DivisionId }).FirstOrDefaultAsync(ct);
            if (work is null || work.DivisionId != divisionOffice.DivisionId)
                return BudgetResponses.Fail<long>(BudgetMessages.NotFound);
            if (work.WorkCode is not null)
                return BudgetResponses.Fail<long>(BudgetMessages.M19);
            if (!await context.AdminApprovals.AsNoTracking().AnyAsync(a => a.WorkId == dto.WorkId, ct))
                return BudgetResponses.Fail<long>(BudgetMessages.M19);

            // At least one (provision, amount) pair; the paired amount is required with its provision.
            var currentPair = dto.CurrentProvisionId is not null && dto.AmountCurrent is not null;
            var nextPair = dto.NextProvisionId is not null && dto.AmountNext is not null;
            if (!currentPair && !nextPair)
                return BudgetResponses.Fail<long>(BudgetMessages.M19);
            if ((dto.CurrentProvisionId is null) != (dto.AmountCurrent is null) ||
                (dto.NextProvisionId is null) != (dto.AmountNext is null))
                return BudgetResponses.Fail<long>(BudgetMessages.M19);

            // Each named provision is an UNFORESEEN provision of the Division's office (BUD-297).
            if (!await ProvisionIsUnforeseenOfOfficeAsync(dto.CurrentProvisionId, divisionOffice.OfficeId, ct) ||
                !await ProvisionIsUnforeseenOfOfficeAsync(dto.NextProvisionId, divisionOffice.OfficeId, ct))
                return BudgetResponses.Fail<long>(BudgetMessages.M19);

            var row = new UnforeseenRequest
            {
                WorkId = dto.WorkId,
                DivisionOfficeId = divisionOffice.OfficeId,
                CurrentProvisionId = dto.CurrentProvisionId,
                AmountCurrent = dto.AmountCurrent,
                NextProvisionId = dto.NextProvisionId,
                AmountNext = dto.AmountNext,
                Status = UnforeseenStatus.Open,
                HolderOfficeId = divisionOffice.OfficeId,
            };
            await using var tx = await scope.BeginAsync("UNFORESEEN_CREATE", ct: ct);
            context.UnforeseenRequests.Add(row);
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new Response<long> { IsSucced = true, Message = "Unforeseen request created", Data = row.UnforeseenRequestId };
        }

        private async Task<bool> ProvisionIsUnforeseenOfOfficeAsync(long? provisionId, long officeId, CancellationToken ct)
        {
            if (provisionId is null)
                return true; // a provision that is not drawn upon is not checked
            return await context.Provisions.AsNoTracking().AnyAsync(p =>
                p.ProvisionId == provisionId.Value && p.HolderType == ProvisionHolderType.Unforeseen &&
                p.OfficeId == officeId && p.SupersededByProvisionId == null, ct);
        }
    }
}
