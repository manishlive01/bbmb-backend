using BBMB.BudgetService.Features.BudgetProcess.AuditTrail.Search.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.ReadRequests;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.AuditTrail.Search
{
    public sealed class SearchAuditTrailEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // F-27: a read of the audit trail. Read via POST: every filter travels in the body, not the URL
            // (security requirement); the former GET /api/audit-trail?… is removed. T-51 has no write path.
            app.MapPost("/api/audit-trail/list",
                    async (AuditTrailListRequestDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(
                            await mediator.Send(new SearchAuditTrailQuery(
                                dto.OfficeId, dto.ActorEmployeeId, dto.From, dto.To, dto.TableName, dto.RecordKey,
                                dto.Page ?? 1, dto.PageSize ?? 50, dto.CycleId), ct),
                            r => Results.Ok((object)r)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("SearchAuditTrail")
                .WithTags("BudgetAuditTrail")
                .Produces<Response<AuditSearchResultDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
