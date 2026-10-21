using BBMB.BudgetService.Features.BudgetProcess.Calendars.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Calendars.Set
{
    /// <summary>
    /// Design §8.5 Set (F-23, BUD-210, BUD-213). Order: actor (M-20) → the cycle exists, else NOT_FOUND. Each
    /// entry is upserted on the (cycle, office_level) PK in one <c>CALENDAR_SET</c> scope, so every change is
    /// audited (T-20 [A], §7). A past date is accepted; it only warns later (BUD-212, M-26). The right F23.Set
    /// is on the endpoint (design §8.1).
    /// </summary>
    public sealed class SetCalendarHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope)
        : IRequestHandler<SetCalendarCommand, Response>
    {
        public const string ActionCode = "CALENDAR_SET";

        public async Task<Response> Handle(SetCalendarCommand request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail(BudgetMessages.M20);
            if (!await context.BudgetCycles.AsNoTracking().AnyAsync(c => c.CycleId == request.CycleId, ct))
                return BudgetResponses.Fail(BudgetMessages.NotFound);

            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            var existing = await context.BudgetCalendarEntries
                .Where(e => e.CycleId == request.CycleId).ToListAsync(ct);
            foreach (var dto in request.Dto.Entries)
            {
                var row = existing.FirstOrDefault(e => e.OfficeLevel == dto.OfficeLevel);
                if (row is null)
                {
                    context.BudgetCalendarEntries.Add(new BudgetCalendarEntry
                    {
                        CycleId = request.CycleId, OfficeLevel = dto.OfficeLevel, LastDate = dto.LastDate,
                    });
                }
                else
                {
                    row.LastDate = dto.LastDate;
                }
            }
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new Response { IsSucced = true, Message = "Calendar saved" };
        }
    }
}
