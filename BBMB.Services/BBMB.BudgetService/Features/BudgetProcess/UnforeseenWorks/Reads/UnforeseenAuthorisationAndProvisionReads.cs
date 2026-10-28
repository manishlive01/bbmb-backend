using System.Text.Json;
using BBMB.BudgetService.Features.BudgetProcess.Offices;
using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Provisions;
using BBMB.BudgetService.Features.BudgetProcess.Provisions.Entities;
using BBMB.BudgetService.Features.BudgetProcess.ReadRequests;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Reads
{
    // BE-GAP-03 (F-22 → F-30, BUD-296, BUD-300): one T-50 authorisation by id, the id the V-10 register carries
    // as entryId for an UNFORESEEN_AUTH row. BE-GAP-01 (F-30, BUD-292, BUD-293, BUD-297): the unforeseen
    // provisions visible to the actor, with the balance available for authorisation. Reads; nothing is written.

    /// <summary>A T-50 authorisation as recorded (BUD-296), with the request it authorised and the provision movements.</summary>
    public sealed class UnforeseenAuthorisationDto
    {
        public long UnforeseenAuthorisationId { get; set; }
        public long UnforeseenRequestId { get; set; }
        public long CeOfficeId { get; set; }
        public string? CeOfficeName { get; set; }
        public decimal? AmountCurrent { get; set; }
        public decimal? AmountNext { get; set; }
        public string Reasons { get; set; } = string.Empty;
        public string WorkCodeGenerated { get; set; } = string.Empty;
        public string RoleCode { get; set; } = string.Empty;
        public DateTimeOffset AuthorisedAt { get; set; }
        public Guid AuthorisedBy { get; set; }
        /// <summary>The proposal as it stood when authorised (T-50 request_snapshot, BUD-296).</summary>
        public JsonElement? RequestSnapshot { get; set; }
        /// <summary>The request as it stands now (same shape as POST /api/unforeseen-requests/detail).</summary>
        public UnforeseenRequestDto? Request { get; set; }
        public List<UnforeseenAuthorisationMovementDto> Movements { get; set; } = [];
    }
    /// <summary>One T-42 movement of the authorisation: UNFORESEEN_OUT on the unforeseen provision, UNFORESEEN_IN on the new work's.</summary>
    public sealed record UnforeseenAuthorisationMovementDto(
        long ProvisionMovementId, long ProvisionId, string MovementType, decimal Amount, string? ProvisionName, int FinYear);

    /// <summary>An UNFORESEEN provision with what was allocated, what was authorised out of it and what is left (BUD-292).</summary>
    public sealed record UnforeseenProvisionDto(
        long ProvisionId, long? CycleId, int FinYear, string Basis, string Category, long OfficeId, string? OfficeName,
        string? Name, bool IsOperative, decimal Allocated, decimal Authorised, decimal ReappropriatedNet, decimal BalanceAvailable);

    public sealed record GetUnforeseenAuthorisationByIdQuery(long UnforeseenAuthorisationId) : IRequest<Response<UnforeseenAuthorisationDto>>;
    public sealed record GetUnforeseenProvisionsQuery(long? CycleId, long? OfficeId) : IRequest<Response<List<UnforeseenProvisionDto>>>;

    /// <summary>
    /// Visible when the actor is Finance or Secretariat, or when the authorising CE office or the requesting
    /// Division is in the actor's visible set (BUD-031); otherwise NOT_FOUND ("not visible to your office").
    /// </summary>
    public sealed class GetUnforeseenAuthorisationByIdHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope, UnforeseenReads reads)
        : IRequestHandler<GetUnforeseenAuthorisationByIdQuery, Response<UnforeseenAuthorisationDto>>
    {
        public async Task<Response<UnforeseenAuthorisationDto>> Handle(GetUnforeseenAuthorisationByIdQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<UnforeseenAuthorisationDto>(BudgetMessages.M20);
            var a = await context.UnforeseenAuthorisations.AsNoTracking()
                .FirstOrDefaultAsync(x => x.UnforeseenAuthorisationId == request.UnforeseenAuthorisationId, ct);
            if (a is null)
                return BudgetResponses.Fail<UnforeseenAuthorisationDto>(BudgetMessages.NotFound);
            var req = await (
                from r in context.UnforeseenRequests.AsNoTracking()
                where r.UnforeseenRequestId == a.UnforeseenRequestId
                join w in context.Works.AsNoTracking() on r.WorkId equals w.Id into wj
                from w in wj.DefaultIfEmpty()
                select new { r.UnforeseenRequestId, r.WorkId, WorkCode = w == null ? null : w.WorkCode, r.DivisionOfficeId,
                             r.Status, r.HolderOfficeId, r.CurrentProvisionId, r.AmountCurrent, r.NextProvisionId, r.AmountNext })
                .FirstOrDefaultAsync(ct);
            if (actor.OfficeLevel is not (OfficeLevel.Finance or OfficeLevel.Secretariat))
            {
                var visible = await officeScope.GetVisibleOfficeIdsAsync(actor.OfficeId, ct);
                var seen = visible.Contains(a.CeOfficeId) || (req is not null && visible.Contains(req.DivisionOfficeId));
                if (!seen)
                    return BudgetResponses.Fail<UnforeseenAuthorisationDto>(BudgetMessages.NotFound);
            }
            var movements = await context.ProvisionMovements.AsNoTracking()
                .Where(m => m.UnforeseenAuthorisationId == a.UnforeseenAuthorisationId)
                .OrderBy(m => m.ProvisionMovementId)
                .Select(m => new { m.ProvisionMovementId, m.ProvisionId, m.MovementType, m.Amount })
                .ToListAsync(ct);
            var described = await ProvisionDescriptions.ResolveAsync(context, movements.Select(m => m.ProvisionId), ct);
            var ceName = (await BudgetOfficeNames.ResolveAsync(context, [a.CeOfficeId], ct)).GetValueOrDefault(a.CeOfficeId);
            JsonElement? snapshot = null;
            try
            {
                using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(a.RequestSnapshot) ? "{}" : a.RequestSnapshot);
                snapshot = doc.RootElement.Clone();
            }
            catch (JsonException)
            {
                snapshot = null;
            }
            var dto = new UnforeseenAuthorisationDto
            {
                UnforeseenAuthorisationId = a.UnforeseenAuthorisationId,
                UnforeseenRequestId = a.UnforeseenRequestId,
                CeOfficeId = a.CeOfficeId,
                CeOfficeName = ceName,
                AmountCurrent = a.AmountCurrent,
                AmountNext = a.AmountNext,
                Reasons = a.Reasons,
                WorkCodeGenerated = a.WorkCodeGenerated,
                RoleCode = a.RoleCode,
                AuthorisedAt = a.CreatedAt,
                AuthorisedBy = a.CreatedBy,
                RequestSnapshot = snapshot,
                Request = req is null
                    ? null
                    : await reads.ToDtoAsync(req.UnforeseenRequestId, req.WorkId, req.WorkCode, req.DivisionOfficeId, req.Status,
                        req.HolderOfficeId, req.CurrentProvisionId, req.AmountCurrent, req.NextProvisionId, req.AmountNext, ct),
                Movements = movements.Select(m =>
                {
                    var d = described.GetValueOrDefault(m.ProvisionId);
                    return new UnforeseenAuthorisationMovementDto(m.ProvisionMovementId, m.ProvisionId, m.MovementType, m.Amount,
                        d?.Name, d?.FinYear ?? 0);
                }).ToList(),
            };
            return new Response<UnforeseenAuthorisationDto> { IsSucced = true, Message = "Unforeseen authorisation", Data = dto };
        }
    }

    /// <summary>
    /// Live (not superseded, V-02) UNFORESEEN provisions in the actor's visible offices (Finance and Secretariat
    /// see all), optionally of one cycle (through the allocation that created them) and one office. Balance
    /// available is the V-02 sum of movements, the figure VR-13 / M-29 checks an authorisation against.
    /// </summary>
    public sealed class GetUnforeseenProvisionsHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope)
        : IRequestHandler<GetUnforeseenProvisionsQuery, Response<List<UnforeseenProvisionDto>>>
    {
        public async Task<Response<List<UnforeseenProvisionDto>>> Handle(GetUnforeseenProvisionsQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<List<UnforeseenProvisionDto>>(BudgetMessages.M20);
            var q =
                from v in context.ProvisionBalances.AsNoTracking()
                where v.HolderType == ProvisionHolderType.Unforeseen
                join p in context.Provisions.AsNoTracking() on v.ProvisionId equals p.ProvisionId
                join al in context.Allocations.AsNoTracking() on p.AllocationId equals (long?)al.AllocationId into alj
                from al in alj.DefaultIfEmpty()
                select new { v, CycleId = al == null ? (long?)null : al.CycleId };
            if (actor.OfficeLevel is not (OfficeLevel.Finance or OfficeLevel.Secretariat))
            {
                var visible = await officeScope.GetVisibleOfficeIdsAsync(actor.OfficeId, ct);
                q = q.Where(x => visible.Contains(x.v.OfficeId));
            }
            if (request.OfficeId is { } officeId)
                q = q.Where(x => x.v.OfficeId == officeId);
            if (request.CycleId is { } cycleId)
                q = q.Where(x => x.CycleId == cycleId);
            var rows = await q.OrderBy(x => x.v.OfficeId).ThenBy(x => x.v.FinYear).ThenBy(x => x.v.ProvisionId).ToListAsync(ct);
            var ids = rows.Select(x => x.v.ProvisionId).ToList();
            var sums = await context.ProvisionMovements.AsNoTracking()
                .Where(m => ids.Contains(m.ProvisionId))
                .GroupBy(m => new { m.ProvisionId, m.MovementType })
                .Select(g => new { g.Key.ProvisionId, g.Key.MovementType, Amount = g.Sum(m => m.Amount) })
                .ToListAsync(ct);
            decimal Sum(long id, params string[] types) =>
                sums.Where(s => s.ProvisionId == id && types.Contains(s.MovementType)).Sum(s => s.Amount);
            var described = await ProvisionDescriptions.ResolveAsync(context, ids, ct);
            var result = rows.Select(x =>
            {
                var d = described.GetValueOrDefault(x.v.ProvisionId);
                return new UnforeseenProvisionDto(
                    x.v.ProvisionId, x.CycleId, x.v.FinYear, x.v.Basis, x.v.Category, x.v.OfficeId, d?.OfficeName, d?.Name,
                    x.v.IsOperative,
                    Allocated: Sum(x.v.ProvisionId, ProvisionMovementType.Allocation),
                    // UNFORESEEN_OUT is written negative on the unforeseen provision; shown here as a positive amount authorised.
                    Authorised: -Sum(x.v.ProvisionId, ProvisionMovementType.UnforeseenOut),
                    ReappropriatedNet: Sum(x.v.ProvisionId, ProvisionMovementType.ReappropIn, ProvisionMovementType.ReappropOut),
                    BalanceAvailable: x.v.Available);
            }).ToList();
            return new Response<List<UnforeseenProvisionDto>> { IsSucced = true, Message = "Unforeseen provisions", Data = result };
        }
    }

    public sealed class UnforeseenAuthorisationAndProvisionEndpoints : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // Read via POST: unforeseenAuthorisationId travels in the body, not the URL (security requirement).
            app.MapPost("/api/unforeseen-authorisations/detail",
                    async (UnforeseenAuthorisationDetailRequestDto body, IMediator m, CancellationToken ct) =>
                        BudgetResults.Ok(await m.Send(new GetUnforeseenAuthorisationByIdQuery(body.UnforeseenAuthorisationId), ct)))
                .RequireAuthorization("BudgetRight:Budget.View").WithName("GetUnforeseenAuthorisationById").WithTags("UnforeseenWorks")
                .Produces<Response<UnforeseenAuthorisationDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
            // Read via POST: the filters travel in the body, not the URL (security requirement).
            app.MapPost("/api/unforeseen-provisions/list",
                    async (UnforeseenProvisionsListRequestDto body, IMediator m, CancellationToken ct) =>
                        BudgetResults.Ok(await m.Send(new GetUnforeseenProvisionsQuery(body.CycleId, body.OfficeId), ct)))
                .RequireAuthorization("BudgetRight:Budget.View").WithName("GetUnforeseenProvisions").WithTags("UnforeseenWorks")
                .Produces<Response<List<UnforeseenProvisionDto>>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
