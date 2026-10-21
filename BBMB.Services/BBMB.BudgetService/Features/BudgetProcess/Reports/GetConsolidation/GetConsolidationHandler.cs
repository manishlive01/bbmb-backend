using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Reports.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Reports.GetConsolidation.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Reports.GetConsolidation
{
    /// <summary>
    /// Design §8.8 (F-18, BUD-177, BUD-266, BUD-311). Order: actor (M-20). Totals of the demanded and distributed
    /// figures of <c>v_report_demand</c> for the cycle (and category / wing filter) are grouped by the requested
    /// key (wing, ce, circle, division or head), WORK and HEAD lines totalled separately and once (BUD-311), the
    /// sums computed in SQL on the unrounded rupee values. Finance and Secretariat see every row; another office
    /// sees only rows of offices in its visible set. The allocated and frozen totals are reported as not yet
    /// available (allocation/freeze arrive with P4, deferred) rather than as a fabricated figure.
    /// </summary>
    public sealed class GetConsolidationHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope)
        : IRequestHandler<GetConsolidationQuery, Response<ConsolidationResultDto>>
    {
        private const string AllocatedNote =
            "Allocated and frozen totals are not yet available: allocation and freeze (P4) are deferred (user time-box 2026-10-15).";

        public async Task<Response<ConsolidationResultDto>> Handle(GetConsolidationQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<ConsolidationResultDto>(BudgetMessages.M20);

            var groupBy = (request.GroupBy ?? "ce").Trim().ToLowerInvariant();
            if (groupBy is not ("wing" or "ce" or "circle" or "division" or "head"))
                return BudgetResponses.Fail<ConsolidationResultDto>(BudgetMessages.M22);

            IQueryable<ReportDemandRow> q = context.ReportDemand.AsNoTracking().Where(r => r.CycleId == request.CycleId);
            if (!string.IsNullOrWhiteSpace(request.Category))
                q = q.Where(r => r.Category == request.Category);
            if (!string.IsNullOrWhiteSpace(request.Wing))
                q = q.Where(r => r.WingCode == request.Wing);

            // Finance and Secretariat see all; others are scoped to their visible offices (BUD-311 scope).
            if (actor.OfficeLevel is not (OfficeLevel.Finance or OfficeLevel.Secretariat))
            {
                var visible = await officeScope.GetVisibleOfficeIdsAsync(actor.OfficeId, ct);
                q = q.Where(r => visible.Contains(r.OriginOfficeId));
            }

            var rows = await q.ToListAsync(ct);
            string KeyOf(ReportDemandRow r) => groupBy switch
            {
                "wing" => r.WingCode ?? "(none)",
                "ce" => r.CeId?.ToString() ?? "(none)",
                "circle" => r.CircleId?.ToString() ?? "(none)",
                "division" => r.DivisionId?.ToString() ?? "(none)",
                _ => r.CompleteCode ?? "(none)",
            };

            var data = rows
                .GroupBy(r => new { Key = KeyOf(r), r.LineType })
                .Select(g => new ConsolidationRowDto(
                    g.Key.Key,
                    groupBy == "head" ? g.First().HeadDesc : null,
                    g.Key.LineType,
                    g.Sum(r => r.DemandedRbe ?? 0m),
                    g.Sum(r => r.DemandedObeNext ?? 0m),
                    g.Sum(r => r.DistributedRbe ?? 0m),
                    g.Sum(r => r.DistributedObeNext ?? 0m)))
                .OrderBy(r => r.LineClass).ThenBy(r => r.Key)
                .ToList();

            return new Response<ConsolidationResultDto>
            {
                IsSucced = true,
                Message = "Consolidation",
                Data = new ConsolidationResultDto(groupBy, false, AllocatedNote, data),
            };
        }
    }
}
