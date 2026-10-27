using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Provisions;
using BBMB.BudgetService.Features.BudgetProcess.Provisions.Entities;
using BBMB.BudgetService.Features.BudgetProcess.ReadRequests;
using BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Entities;
using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Reads
{
    // Design §8.7 reads (F-22; BUD-207, BUD-208, BUD-209, BUD-300). GetList/GetById return an entry with its
    // sources and each provision's available figure; GetNewWorkCandidates lists works eligible as a new-work
    // destination; GetRegister reads V-10, with the unforeseen authorisations marked. All reads; nothing written.

    public sealed record GetReappropriationListQuery(int? FinYear) : IRequest<List<ReappropriationDto>>;
    public sealed record GetReappropriationByIdQuery(long Id) : IRequest<ReappropriationDto?>;
    /// <summary>The surrender sources of one entry, in the shape of <see cref="ReappropriationDto.Sources"/>; null = no such entry.</summary>
    public sealed record GetReappropriationSourcesQuery(long ReappropriationId) : IRequest<List<ReappropriationSourceDto>?>;
    public sealed record GetNewWorkCandidatesQuery : IRequest<List<NewWorkCandidateDto>>;
    public sealed record GetReappropriationRegisterQuery(int? FinYear, long? OfficeId) : IRequest<List<ReappropriationRegisterDto>>;

    public sealed class ReappropriationReads(BudgetContext context)
    {
        public async Task<ReappropriationDto> LoadAsync(Reappropriation r, CancellationToken ct)
        {
            var sources = await context.ReappropriationSources.AsNoTracking()
                .Where(s => s.ReappropriationId == r.ReappropriationId)
                .Select(s => new { s.ReappropriationSourceId, s.ProvisionId, s.Amount, s.IsRemoved })
                .ToListAsync(ct);
            var provisionIds = sources.Select(s => s.ProvisionId)
                .Concat(r.DestinationProvisionId is { } d ? [d] : Array.Empty<long>()).Distinct().ToList();
            var available = provisionIds.Count == 0
                ? new Dictionary<long, decimal>()
                : await context.ProvisionBalances.AsNoTracking()
                    .Where(v => provisionIds.Contains(v.ProvisionId))
                    .ToDictionaryAsync(v => v.ProvisionId, v => v.Available, ct);
            // BE-GAP-02 (BUD-207): names, codes and the before/after figures of every provision the entry touches.
            var described = await ProvisionDescriptions.ResolveAsync(context, provisionIds, ct);
            var ledger = await context.ProvisionMovements.AsNoTracking()
                    .Where(m => provisionIds.Contains(m.ProvisionId))
                    .Select(m => new { m.ProvisionMovementId, m.ProvisionId, m.Amount, m.ReappropriationId })
                    .ToListAsync(ct);
            var basis = r.Status switch
            {
                ReappropriationStatus.Submitted => "RECORDED",
                ReappropriationStatus.Open => "PROJECTED",
                _ => null,
            };
            (decimal? Before, decimal? After) Figures(long? provisionId, decimal delta)
            {
                if (basis is null)
                    return (null, null);
                if (provisionId is null)
                    return (0m, delta); // a new work: no provision until the entry is submitted (nil before)
                var own = ledger.Where(m => m.ProvisionId == provisionId && m.ReappropriationId == r.ReappropriationId).ToList();
                if (basis == "RECORDED" && own.Count > 0)
                {
                    var first = own.Min(m => m.ProvisionMovementId);
                    var before = ledger.Where(m => m.ProvisionId == provisionId && m.ProvisionMovementId < first).Sum(m => m.Amount);
                    return (before, before + own.Sum(m => m.Amount));
                }
                var now = ledger.Where(m => m.ProvisionId == provisionId).Sum(m => m.Amount);
                return (now, now + (basis == "PROJECTED" ? delta : 0m));
            }
            var dest = r.DestinationProvisionId is { } dp ? described.GetValueOrDefault(dp) : null;
            var newWork = dest is null && r.DestinationNewWorkId is { } nw
                ? await context.Works.AsNoTracking().Where(w => w.Id == nw)
                    .Select(w => new { w.WorkDesc, w.WorkCode }).FirstOrDefaultAsync(ct)
                : null;
            var destFigures = Figures(r.DestinationProvisionId, r.AdditionalAmount);
            return new ReappropriationDto
            {
                FiguresBasis = basis,
                DestinationHolderType = dest?.HolderType ?? (r.DestinationNewWorkId is null ? null : ProvisionHolderType.Work),
                DestinationName = dest?.Name ?? newWork?.WorkDesc,
                DestinationWorkCode = dest?.WorkCode ?? newWork?.WorkCode,
                DestinationAccountCodeId = dest?.AccountCodeId,
                DestinationAccountCode = dest?.CompleteCode,
                DestinationOfficeId = dest?.OfficeId,
                DestinationOfficeName = dest?.OfficeName,
                DestinationExistingBudget = destFigures.Before,
                DestinationAfter = destFigures.After,
                DestinationAvailableNow = r.DestinationProvisionId is { } da && available.TryGetValue(da, out var dav) ? dav : null,
                ReappropriationId = r.ReappropriationId,
                FinYear = r.FinYear,
                SerialNo = r.SerialNo,
                Status = r.Status,
                DestinationProvisionId = r.DestinationProvisionId,
                DestinationNewWorkId = r.DestinationNewWorkId,
                AdditionalAmount = r.AdditionalAmount,
                EofficeOrderRef = r.EofficeOrderRef,
                Remarks = r.Remarks,
                OrderAttachmentId = r.OrderAttachmentId,
                Sources = sources.Select(s =>
                {
                    var d = described.GetValueOrDefault(s.ProvisionId);
                    var f = Figures(s.ProvisionId, s.IsRemoved ? 0m : -s.Amount);
                    return new ReappropriationSourceDto(
                        s.ReappropriationSourceId, s.ProvisionId, s.Amount, s.IsRemoved,
                        available.TryGetValue(s.ProvisionId, out var a) ? a : null,
                        d?.HolderType, d?.Name, d?.WorkCode, d?.AccountCodeId, d?.CompleteCode, d?.OfficeId, d?.OfficeName,
                        f.Before, f.After);
                }).ToList(),
            };
        }
    }

    public sealed class GetReappropriationListHandler(BudgetContext context, IBudgetActorSource actorSource, ReappropriationReads reads)
        : IRequestHandler<GetReappropriationListQuery, List<ReappropriationDto>>
    {
        public async Task<List<ReappropriationDto>> Handle(GetReappropriationListQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return [];
            var query = context.Reappropriations.AsNoTracking().AsQueryable();
            if (request.FinYear is { } y)
                query = query.Where(r => r.FinYear == y);
            var rows = await query.OrderByDescending(r => r.FinYear).ThenByDescending(r => r.SerialNo).ToListAsync(ct);
            var result = new List<ReappropriationDto>();
            foreach (var r in rows)
                result.Add(await reads.LoadAsync(r, ct));
            return result;
        }
    }

    public sealed class GetReappropriationByIdHandler(BudgetContext context, ReappropriationReads reads)
        : IRequestHandler<GetReappropriationByIdQuery, ReappropriationDto?>
    {
        public async Task<ReappropriationDto?> Handle(GetReappropriationByIdQuery request, CancellationToken ct)
        {
            var r = await context.Reappropriations.AsNoTracking().FirstOrDefaultAsync(x => x.ReappropriationId == request.Id, ct);
            return r is null ? null : await reads.LoadAsync(r, ct);
        }
    }

    // Same lookup as GetReappropriationByIdHandler, so the sources list and the detail can never disagree.
    public sealed class GetReappropriationSourcesHandler(BudgetContext context, ReappropriationReads reads)
        : IRequestHandler<GetReappropriationSourcesQuery, List<ReappropriationSourceDto>?>
    {
        public async Task<List<ReappropriationSourceDto>?> Handle(GetReappropriationSourcesQuery request, CancellationToken ct)
        {
            var r = await context.Reappropriations.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ReappropriationId == request.ReappropriationId, ct);
            return r is null ? null : (await reads.LoadAsync(r, ct)).Sources;
        }
    }
    public sealed class GetNewWorkCandidatesHandler(BudgetContext context)
        : IRequestHandler<GetNewWorkCandidatesQuery, List<NewWorkCandidateDto>>
    {
        public async Task<List<NewWorkCandidateDto>> Handle(GetNewWorkCandidatesQuery request, CancellationToken ct) =>
            await context.Works.AsNoTracking()
                .Where(w => w.WorkCode == null && !context.Provisions.Any(p => p.WorkId == w.Id))
                .OrderBy(w => w.Id)
                .Select(w => new NewWorkCandidateDto(w.Id, w.WorkDesc, w.DivisionId))
                .ToListAsync(ct);
    }

    public sealed class GetReappropriationRegisterHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope)
        : IRequestHandler<GetReappropriationRegisterQuery, List<ReappropriationRegisterDto>>
    {
        public async Task<List<ReappropriationRegisterDto>> Handle(GetReappropriationRegisterQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return [];
            var query = context.ReappropriationRegister.AsNoTracking().AsQueryable();
            if (request.FinYear is { } y)
                query = query.Where(r => r.FinYear == y);
            if (request.OfficeId is { } o)
                query = query.Where(r => r.OfficeId == o);

            var seesAll = actor.OfficeLevel is OfficeLevel.Finance or OfficeLevel.Secretariat;
            if (!seesAll)
            {
                var visible = await officeScope.GetVisibleOfficeIdsAsync(actor.OfficeId, ct);
                query = query.Where(r => visible.Contains(r.OfficeId));
            }
            return await query.OrderBy(r => r.FinYear).ThenBy(r => r.CreatedAt)
                .Select(r => new ReappropriationRegisterDto
                {
                    EntryKind = r.EntryKind, EntryId = r.EntryId, FinYear = r.FinYear, SerialNo = r.SerialNo,
                    Status = r.Status, Amount = r.Amount, EofficeRef = r.EofficeRef, OfficeId = r.OfficeId, CreatedAt = r.CreatedAt,
                }).ToListAsync(ct);
        }
    }

    public sealed class ReappropriationReadEndpoints : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/reappropriations",
                    async (int? finYear, IMediator m, CancellationToken ct) => Results.Ok(await m.Send(new GetReappropriationListQuery(finYear), ct)))
                .RequireAuthorization("BudgetRight:Budget.View").WithName("GetReappropriationList").WithTags("Reappropriations")
                .Produces<List<ReappropriationDto>>(StatusCodes.Status200OK);

            app.MapGet("/api/reappropriations/new-work-candidates",
                    async (IMediator m, CancellationToken ct) => Results.Ok(await m.Send(new GetNewWorkCandidatesQuery(), ct)))
                .RequireAuthorization("BudgetRight:Budget.View").WithName("GetNewWorkCandidates").WithTags("Reappropriations")
                .Produces<List<NewWorkCandidateDto>>(StatusCodes.Status200OK);

            app.MapGet("/api/reappropriations/register",
                    async (int? finYear, long? officeId, IMediator m, CancellationToken ct) =>
                        Results.Ok(await m.Send(new GetReappropriationRegisterQuery(finYear, officeId), ct)))
                .RequireAuthorization("BudgetRight:Budget.View").WithName("GetReappropriationRegister").WithTags("Reappropriations")
                .Produces<List<ReappropriationRegisterDto>>(StatusCodes.Status200OK);

            // Read via POST: reappropriationId travels in the body, not the URL (security requirement).
            app.MapPost("/api/reappropriations/detail",
                    async (ReappropriationDetailRequestDto body, IMediator m, CancellationToken ct) =>
                    {
                        var dto = await m.Send(new GetReappropriationByIdQuery(body.ReappropriationId), ct);
                        return dto is null ? Results.NotFound() : Results.Ok(dto);
                    })
                .RequireAuthorization("BudgetRight:Budget.View").WithName("GetReappropriationById").WithTags("Reappropriations")
                .Produces<ReappropriationDto>(StatusCodes.Status200OK).Produces(StatusCodes.Status404NotFound)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
            // Read via POST, under /sources/list so it cannot clash with POST /api/reappropriations/{id}/sources (add).
            app.MapPost("/api/reappropriations/sources/list",
                    async (ReappropriationDetailRequestDto body, IMediator m, CancellationToken ct) =>
                    {
                        var sources = await m.Send(new GetReappropriationSourcesQuery(body.ReappropriationId), ct);
                        return sources is null ? Results.NotFound() : Results.Ok(sources);
                    })
                .RequireAuthorization("BudgetRight:Budget.View").WithName("GetReappropriationSources").WithTags("Reappropriations")
                .Produces<List<ReappropriationSourceDto>>(StatusCodes.Status200OK).Produces(StatusCodes.Status404NotFound)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
