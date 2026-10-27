using BBMB.BudgetService.Features.BudgetProcess.AuditTrail.Search.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.AuditTrail.Search
{
    /// <summary>
    /// Design §8.8 (<c>POST /api/audit-trail/list</c>, F-27): a paged search of T-51, filtered by office, actor,
    /// time window, table, record and budget cycle (BUD-228), scoped to the actor's visible offices. A read only
    /// (BUD-229). <see cref="CycleId"/> is additive: null = no cycle filter.
    /// </summary>
    public sealed record SearchAuditTrailQuery(
        long? OfficeId,
        Guid? ActorEmployeeId,
        DateTimeOffset? From,
        DateTimeOffset? To,
        string? TableName,
        string? RecordKey,
        int Page,
        int PageSize,
        long? CycleId = null) : IRequest<Response<AuditSearchResultDto>>;
}
