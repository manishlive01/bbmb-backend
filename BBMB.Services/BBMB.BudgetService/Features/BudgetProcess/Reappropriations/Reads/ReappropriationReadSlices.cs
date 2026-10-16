using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Entities;
using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Endpoints;
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
            return new ReappropriationDto
            {
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
                Sources = sources.Select(s => new ReappropriationSourceDto(
                    s.ReappropriationSourceId, s.ProvisionId, s.Amount, s.IsRemoved,
                    available.TryGetValue(s.ProvisionId, out var a) ? a : null)).ToList(),
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

            app.MapGet("/api/reappropriations/{id:long}",
                    async (long id, IMediator m, CancellationToken ct) =>
                    {
                        var dto = await m.Send(new GetReappropriationByIdQuery(id), ct);
                        return dto is null ? Results.NotFound() : Results.Ok(dto);
                    })
                .RequireAuthorization("BudgetRight:Budget.View").WithName("GetReappropriationById").WithTags("Reappropriations")
                .Produces<ReappropriationDto>(StatusCodes.Status200OK).Produces(StatusCodes.Status404NotFound);
        }
    }
}
