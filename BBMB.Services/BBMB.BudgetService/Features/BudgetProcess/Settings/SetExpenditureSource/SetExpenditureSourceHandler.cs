using BBMB.BudgetService.Features.BudgetProcess.Settings.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.FinancialAccounting;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Settings.SetExpenditureSource
{
    /// <summary>
    /// Design §8.2, §4.11 (BUD-025, BUD-312). Order: actor (M-20) → the office exists, else NOT_FOUND → mode
    /// DRAWN while Financial Accounting is the not-available stub is refused (BUD-312: no column is closed to
    /// entry while the module holds no source to draw from) → one T-18 row is appended with the instant as
    /// <c>effective_from</c>, in a scope (append-only; the earlier row is kept). The right Settings.ExpenditureSource
    /// (IT Cell only) is on the endpoint.
    /// </summary>
    public sealed class SetExpenditureSourceHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope,
        IFinancialAccountingGateway gateway, TimeProvider? clock = null)
        : IRequestHandler<SetExpenditureSourceCommand, Response>
    {
        public const string ActionCode = "EXPENDITURE_SOURCE_SET";

        public async Task<Response> Handle(SetExpenditureSourceCommand request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail(BudgetMessages.M20);
            var dto = request.Dto;
            if (!await context.BudgetOffices.AsNoTracking().AnyAsync(o => o.OfficeId == dto.OfficeId, ct))
                return BudgetResponses.Fail(BudgetMessages.NotFound);
            // BUD-312: DRAWN is refused while no source answers (the gateway is the not-available stub). Keying
            // must stay open while the module holds no source to draw from (§4.11).
            if (dto.Mode == ExpenditureSourceMode.Drawn && gateway is NotAvailableFinancialAccountingGateway)
                return new Response
                {
                    IsSucced = false,
                    Message = "DRAWN cannot be set while Financial Accounting is not available; the figures would have no source to draw from (BUD-312). Keep the mode KEYED until the interface is live.",
                };

            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            context.ExpenditureSourceSettings.Add(new ExpenditureSourceSetting
            {
                OfficeId = dto.OfficeId, FinYear = dto.FinYear, Mode = dto.Mode,
                EffectiveFrom = (clock ?? TimeProvider.System).GetUtcNow(),
            });
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new Response { IsSucced = true, Message = "Expenditure source set" };
        }
    }
}
