using BBMB.BudgetService.Features.BudgetProcess.Calendars.Get.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Calendars.Get
{
    /// <summary>
    /// Design §8.5 Get (F-23, BUD-211). Order: actor (M-20) → the cycle exists, else NOT_FOUND. Returns every
    /// entry of the cycle and, for the actor's own office level, the days remaining (negative = overdue) against
    /// its last date computed in IST, and the number of proposals the actor's office currently holds (the work
    /// still before it). A level with no row has null dates and is not overdue.
    /// </summary>
    public sealed class GetCalendarHandler(BudgetContext context, IBudgetActorSource actorSource, TimeProvider? clock = null)
        : IRequestHandler<GetCalendarQuery, Response<CalendarViewDto>>
    {
        private static readonly TimeZoneInfo IndiaStandardTime = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");

        public async Task<Response<CalendarViewDto>> Handle(GetCalendarQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<CalendarViewDto>(BudgetMessages.M20);
            if (!await context.BudgetCycles.AsNoTracking().AnyAsync(c => c.CycleId == request.CycleId, ct))
                return BudgetResponses.Fail<CalendarViewDto>(BudgetMessages.NotFound);

            var entries = await context.BudgetCalendarEntries.AsNoTracking()
                .Where(e => e.CycleId == request.CycleId)
                .OrderBy(e => e.OfficeLevel)
                .Select(e => new CalendarEntryViewDto(e.OfficeLevel, e.LastDate))
                .ToListAsync(ct);

            var actorEntry = entries.FirstOrDefault(e => e.OfficeLevel == actor.OfficeLevel);
            var todayIst = DateOnly.FromDateTime(
                TimeZoneInfo.ConvertTime((clock ?? TimeProvider.System).GetUtcNow(), IndiaStandardTime).DateTime);
            int? daysRemaining = actorEntry is null ? null : actorEntry.LastDate.DayNumber - todayIst.DayNumber;
            var overdue = daysRemaining is { } d && d < 0;

            var pendingCount = await context.BudgetProposals.AsNoTracking()
                .CountAsync(p => p.CycleId == request.CycleId && p.HolderOfficeId == actor.OfficeId, ct);

            return new Response<CalendarViewDto>
            {
                IsSucced = true,
                Message = "Budget calendar",
                Data = new CalendarViewDto(
                    request.CycleId, entries, actor.OfficeLevel, actorEntry?.LastDate, daysRemaining, overdue, pendingCount),
            };
        }
    }
}
