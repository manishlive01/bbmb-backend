using BBMB.BudgetService.Features.BudgetProcess.AuditTrail.Search.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.AuditTrail.Search
{
    /// <summary>
    /// Design §8.8 (<c>GET /api/audit-trail</c>, F-27): a paged search of T-51, filtered by office, actor,
    /// time window, table and record, scoped to the actor's visible offices. GET only.
    /// </summary>
    public sealed record SearchAuditTrailQuery(
        long? OfficeId,
        Guid? ActorEmployeeId,
        DateTimeOffset? From,
        DateTimeOffset? To,
        string? TableName,
        string? RecordKey,
        int Page,
        int PageSize) : IRequest<Response<AuditSearchResultDto>>;
}
