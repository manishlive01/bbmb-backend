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
    /// time window, table and record. Rows are returned newest first, paged, with the total matched. The
    /// before/after values are the T-51 <c>old_value</c>/<c>new_value</c> (BUD-215). GET only (the endpoint maps
    /// no other verb); T-51 is append-only (fn_forbid_change), so no write path exists in any case.
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
    }
}
