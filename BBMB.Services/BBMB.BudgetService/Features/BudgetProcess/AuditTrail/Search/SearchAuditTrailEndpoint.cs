using BBMB.BudgetService.Features.BudgetProcess.AuditTrail.Search.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.AuditTrail.Search
{
    public sealed class SearchAuditTrailEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // F-27: a read of the audit trail. GET only — no other verb is mapped under /api/audit-trail.
            app.MapGet("/api/audit-trail",
                    async (long? officeId, Guid? actorEmployeeId, DateTimeOffset? from, DateTimeOffset? to,
                           string? tableName, string? recordKey, int? page, int? pageSize,
                           IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(
                            await mediator.Send(new SearchAuditTrailQuery(
                                officeId, actorEmployeeId, from, to, tableName, recordKey, page ?? 1, pageSize ?? 50), ct),
                            r => Results.Ok((object)r)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("SearchAuditTrail")
                .WithTags("BudgetAuditTrail")
                .Produces<Response<AuditSearchResultDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden);
        }
    }
}
