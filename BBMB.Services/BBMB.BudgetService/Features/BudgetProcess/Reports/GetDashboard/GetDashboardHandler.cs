using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Reports.GetDashboard.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Reports.GetDashboard
{
    /// <summary>
    /// Design §8.8 (F-25, BUD-219). Order: actor (M-20). Reports, for the cycle: proposals grouped by category
    /// and state (scoped to the actor's visible offices unless Finance/Secretariat); the proposals the actor's
    /// office currently holds, each with the route to open it; the calendar status for the actor's level (days
    /// remaining in IST, from T-20); and, for Finance and Secretariat, the number of open correction flags
    /// (M-34). Every figure is a live count or an existing-table read — none depends on allocation/freeze, so no
    /// 'not yet available' field arises here.
    /// </summary>
    public sealed class GetDashboardHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope, TimeProvider? clock = null)
        : IRequestHandler<GetDashboardQuery, Response<DashboardDto>>
    {
        private static readonly TimeZoneInfo IndiaStandardTime = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");

        public async Task<Response<DashboardDto>> Handle(GetDashboardQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<DashboardDto>(BudgetMessages.M20);

            var seesAll = actor.OfficeLevel is OfficeLevel.Finance or OfficeLevel.Secretariat;
            var visible = seesAll ? null : await officeScope.GetVisibleOfficeIdsAsync(actor.OfficeId, ct);

            var proposals = context.BudgetProposals.AsNoTracking().Where(p => p.CycleId == request.CycleId);
            var scoped = seesAll ? proposals : proposals.Where(p => visible!.Contains(p.OriginOfficeId) || visible!.Contains(p.HolderOfficeId));

            var byState = await scoped
                .GroupBy(p => new { p.Category, p.StateCode })
                .Select(g => new CategoryStateCountDto(g.Key.Category, g.Key.StateCode, g.Count()))
                .ToListAsync(ct);

            var pending = await proposals
                .Where(p => p.HolderOfficeId == actor.OfficeId)
                .OrderBy(p => p.Category).ThenBy(p => p.ProposalId)
                .Select(p => new PendingProposalDto(p.ProposalId, p.Category, p.StateCode, $"/api/budget-proposals/{p.ProposalId}"))
                .ToListAsync(ct);

            var lastDate = await context.BudgetCalendarEntries.AsNoTracking()
                .Where(e => e.CycleId == request.CycleId && e.OfficeLevel == actor.OfficeLevel)
                .Select(e => (DateOnly?)e.LastDate).FirstOrDefaultAsync(ct);
            var todayIst = DateOnly.FromDateTime(
                TimeZoneInfo.ConvertTime((clock ?? TimeProvider.System).GetUtcNow(), IndiaStandardTime).DateTime);
            int? daysRemaining = lastDate is null ? null : lastDate.Value.DayNumber - todayIst.DayNumber;
            var calendar = new CalendarStatusDto(actor.OfficeLevel, lastDate, daysRemaining, daysRemaining is { } d && d < 0);

            int? openFlags = seesAll
                ? await context.CorrectionFlags.AsNoTracking().CountAsync(f => f.ClearedAt == null, ct)
                : null;

            return new Response<DashboardDto>
            {
                IsSucced = true,
                Message = "Dashboard",
                Data = new DashboardDto(request.CycleId, byState, pending, calendar, openFlags),
            };
        }
    }
}
