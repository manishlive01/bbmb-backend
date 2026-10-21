using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Offices.SetWing
{
    /// <summary>
    /// Design §8.2: corrects the wing of an existing CE office (the wing is first set at load,
    /// implementation-time change of 2026-10-14). Absent office → NOT_FOUND; not a CE → M-19; wing not in
    /// irr_pwr → M-05. The change is audited by fn_audit_row under action SET_WING (old and new value).
    /// </summary>
    public sealed class SetOfficeWingHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope)
        : IRequestHandler<SetOfficeWingCommand, Response>
    {
        public const string ActionCode = "SET_WING";

        public async Task<Response> Handle(SetOfficeWingCommand request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail(BudgetMessages.M20);

            var office = await context.BudgetOffices.FirstOrDefaultAsync(o => o.OfficeId == request.OfficeId, ct);
            if (office is null)
                return BudgetResponses.Fail(BudgetMessages.NotFound);
            if (office.OfficeLevel != OfficeLevel.Ce)
                return BudgetResponses.Fail(BudgetMessages.M19);

            var wing = request.Dto.WingCode.Trim();
            if (!await context.PowerIrrigations.AsNoTracking().AnyAsync(p => p.IrrPwrCode == wing, ct))
                return BudgetResponses.Fail(BudgetMessages.M05);

            if (office.WingCode != wing)
            {
                office.WingCode = wing;
                await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
                await context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            return new Response { IsSucced = true, Message = "Wing set" };
        }
    }
}
