using BBMB.BudgetService.Features.BudgetProcess.AuditTrail.Entities;
using BBMB.BudgetService.Features.BudgetProcess.AuditTrail.Search.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.AuditTrail.Search
{
    /// <summary>
    /// Design §8.8 (F-27). Order: actor (M-20). The search is scoped to T-51 rows whose <c>actor_office_id</c>
    /// is in the actor's visible set (another CE's rows are absent), then filtered by the given office, actor,
    /// time window, table, record and budget cycle (BUD-228). Rows are returned newest first, paged, with the total matched. The
    /// before/after values are the T-51 <c>old_value</c>/<c>new_value</c> (BUD-215). A read only, POST /api/audit-trail/list
    /// (no write verb is mapped); T-51 is append-only (fn_forbid_change), so no write path exists in any case.
    /// </summary>
    public sealed class SearchAuditTrailHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope)
        : IRequestHandler<SearchAuditTrailQuery, Response<AuditSearchResultDto>>
    {
        private const int MaxPageSize = 200;

        public async Task<Response<AuditSearchResultDto>> Handle(SearchAuditTrailQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<AuditSearchResultDto>(BudgetMessages.M20);
            var visible = await officeScope.GetVisibleOfficeIdsAsync(actor.OfficeId, ct);

            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize is < 1 or > MaxPageSize ? 50 : request.PageSize;

            var q = context.AuditLog.AsNoTracking().Where(a => visible.Contains(a.ActorOfficeId));
            if (request.OfficeId is { } officeId)
                q = q.Where(a => a.ActorOfficeId == officeId);
            if (request.ActorEmployeeId is { } emp)
                q = q.Where(a => a.ActorEmployeeId == emp);
            if (request.From is { } from)
                q = q.Where(a => a.OccurredAt >= from);
            if (request.To is { } to)
                q = q.Where(a => a.OccurredAt <= to);
            if (!string.IsNullOrWhiteSpace(request.TableName))
                q = q.Where(a => a.TableName == request.TableName);
            if (!string.IsNullOrWhiteSpace(request.RecordKey))
                q = q.Where(a => a.RecordKey == request.RecordKey);
            if (request.CycleId is { } cycleId)
                q = WhereBelongsToCycle(q, cycleId);

            var total = await q.LongCountAsync(ct);
            var rows = await q
                .OrderByDescending(a => a.OccurredAt).ThenByDescending(a => a.AuditId)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(a => new AuditRowDto(
                    a.AuditId, a.OccurredAt, a.TableName, a.RecordKey, a.Operation, a.ColumnName,
                    a.OldValue, a.NewValue, a.ActionCode, a.ActorEmployeeId, a.ActorName, a.ActorOfficeId, a.ActorRoleCode))
                .ToListAsync(ct);

            return new Response<AuditSearchResultDto>
            {
                IsSucced = true,
                Message = "Audit trail",
                Data = new AuditSearchResultDto(page, pageSize, total, rows),
            };
        }

        /// <summary>
        /// BUD-228 search by budget cycle (BE-GAP-05). A T-51 row belongs to a cycle when the record it audits is
        /// one of that cycle's records: the cycle itself, its calendar and freeze rows, its proposals and their
        /// demand lines, figure versions, observations and movements, its CE recommendations and distributions,
        /// its allocations and the provisions they created. Matched on (table_name, record_key), record_key
        /// being the audited row's primary key as text (fn_audit_row). Every set is a sub-query, nothing is
        /// loaded. Not matched: rows of tables that carry no cycle (masters, settings, works, sanctions,
        /// re-appropriations, unforeseen requests) — by design, as they do not belong to one cycle.
        /// </summary>
        private IQueryable<AuditLogEntry> WhereBelongsToCycle(
            IQueryable<AuditLogEntry> q, long cycleId)
        {
            var cycleKey = cycleId.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var calendarPrefix = cycleKey + "|";
            var proposalIds = context.BudgetProposals.Where(p => p.CycleId == cycleId).Select(p => p.ProposalId);
            var lineIds = context.DemandLines.Where(d => proposalIds.Contains(d.ProposalId)).Select(d => d.DemandLineId);
            var recommendationIds = context.CeRecommendations.Where(r => r.CycleId == cycleId).Select(r => r.RecommendationId);
            var allocationIds = context.Allocations.Where(a => a.CycleId == cycleId).Select(a => a.AllocationId);
            var proposalKeys = proposalIds.Select(id => id.ToString());
            var lineKeys = lineIds.Select(id => id.ToString());
            var figureKeys = context.FigureVersions.Where(f => lineIds.Contains(f.DemandLineId)).Select(f => f.FigureVersionId.ToString());
            var observationKeys = context.Observations.Where(o => proposalIds.Contains(o.ProposalId)).Select(o => o.ObservationId.ToString());
            var movementKeys = context.ProposalMovements.Where(m => proposalIds.Contains(m.ProposalId)).Select(m => m.MovementId.ToString());
            var recommendationKeys = recommendationIds.Select(id => id.ToString());
            var distributionKeys = context.Distributions.Where(d => recommendationIds.Contains(d.RecommendationId)).Select(d => d.DistributionId.ToString());
            var allocationKeys = allocationIds.Select(id => id.ToString());
            var provisionKeys = context.Provisions
                .Where(p => p.AllocationId != null && allocationIds.Contains(p.AllocationId.Value)).Select(p => p.ProvisionId.ToString());
            return q.Where(a =>
                ((a.TableName == "budget_cycle" || a.TableName == "freeze") && a.RecordKey == cycleKey)
                || (a.TableName == "calendar" && a.RecordKey.StartsWith(calendarPrefix))
                || (a.TableName == "proposal" && proposalKeys.Contains(a.RecordKey))
                || (a.TableName == "demand_line" && lineKeys.Contains(a.RecordKey))
                || (a.TableName == "figure_version" && figureKeys.Contains(a.RecordKey))
                || (a.TableName == "observation" && observationKeys.Contains(a.RecordKey))
                || (a.TableName == "proposal_movement" && movementKeys.Contains(a.RecordKey))
                || (a.TableName == "ce_recommendation" && recommendationKeys.Contains(a.RecordKey))
                || (a.TableName == "distribution" && distributionKeys.Contains(a.RecordKey))
                || (a.TableName == "allocation" && allocationKeys.Contains(a.RecordKey))
                || (a.TableName == "provision" && provisionKeys.Contains(a.RecordKey)));
        }
    }
}
