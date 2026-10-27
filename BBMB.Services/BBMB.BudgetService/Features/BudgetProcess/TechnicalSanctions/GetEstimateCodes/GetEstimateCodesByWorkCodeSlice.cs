using BBMB.BudgetService.Features.BudgetProcess.ReadRequests;
using BBMB.BudgetService.Features.Works.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.GetEstimateCodes
{
    /// <summary>
    /// One estimate code of a work (BUD-270/271): the code recorded on a technical sanction (T-27 column 11,
    /// written by fn_record_estimate_code), with that sanction's current-version amount and date.
    /// </summary>
    public sealed record EstimateCodeDto(
        string EstimateCode, long TechnicalSanctionId, decimal? Amount, DateOnly? Date, DateTimeOffset? EstimateCodeAt);
    public sealed record GetEstimateCodesByWorkCodeQuery(string WorkCode) : IRequest<Response<List<EstimateCodeDto>>>;
    /// <summary>
    /// Resolves the work by T-23 work_code (either form, held as text) within the actor's visible Divisions,
    /// exactly as GetWorkByCode does (§4.9, BUD-031); unknown or not visible is NOT_FOUND. Sanctions without an
    /// estimate code yet are left out. The legacy <c>estimate</c> table is not read: after the P4 reset the
    /// estimate code per technical sanction lives on T-27.
    /// </summary>
    public sealed class GetEstimateCodesByWorkCodeHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope, WorkRules rules)
        : IRequestHandler<GetEstimateCodesByWorkCodeQuery, Response<List<EstimateCodeDto>>>
    {
        public async Task<Response<List<EstimateCodeDto>>> Handle(GetEstimateCodesByWorkCodeQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<List<EstimateCodeDto>>(BudgetMessages.M20);
            var visibleDivisions = await rules.GetVisibleDivisionIdsAsync(actorSource, officeScope, ct);
            var workCode = request.WorkCode.Trim();
            var workId = await context.Works.AsNoTracking()
                .Where(w => w.WorkCode == workCode && visibleDivisions.Contains(w.DivisionId))
                .Select(w => (int?)w.Id).FirstOrDefaultAsync(ct);
            if (workId is null)
                return BudgetResponses.Fail<List<EstimateCodeDto>>(BudgetMessages.NotFound);
            var sanctions = await context.TechnicalSanctions.AsNoTracking()
                .Where(t => t.WorkId == workId && t.EstimateCode != null)
                .OrderBy(t => t.SerialNo)
                .Select(t => new { t.TechnicalSanctionId, t.CurrentVersionNo, EstimateCode = t.EstimateCode!, t.EstimateCodeAt })
                .ToListAsync(ct);
            var ids = sanctions.Select(s => s.TechnicalSanctionId).ToList();
            var versions = await context.TechnicalSanctionVersions.AsNoTracking()
                .Where(v => ids.Contains(v.TechnicalSanctionId))
                .Select(v => new { v.TechnicalSanctionId, v.VersionNo, v.TsAmount, v.TsDate })
                .ToListAsync(ct);
            var current = versions.ToDictionary(v => (v.TechnicalSanctionId, v.VersionNo));
            return new Response<List<EstimateCodeDto>>
            {
                IsSucced = true, Message = "Estimate codes",
                Data = sanctions.Select(s =>
                {
                    var v = current.GetValueOrDefault((s.TechnicalSanctionId, s.CurrentVersionNo));
                    return new EstimateCodeDto(s.EstimateCode, s.TechnicalSanctionId, v?.TsAmount, v?.TsDate, s.EstimateCodeAt);
                }).ToList(),
            };
        }
    }
    public sealed class GetEstimateCodesByWorkCodeEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // Read via POST: the work code travels in the body, not the URL (security requirement).
            app.MapPost("/api/work-codes/estimate-codes", async (
                    WorkCodeRequestDto dto, IMediator mediator, CancellationToken ct) =>
                    BudgetResults.Ok(await mediator.Send(new GetEstimateCodesByWorkCodeQuery(dto.WorkCode), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetEstimateCodesByWorkCode")
                .WithTags("TechnicalSanctions")
                .Produces<Response<List<EstimateCodeDto>>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
