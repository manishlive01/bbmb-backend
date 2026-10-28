using BBMB.BudgetService.Features.BudgetProcess.ReadRequests;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Reads
{
    // Design §8.6 GET unforeseen-requests and /{id}: the requests pending with the acting office and one by id,
    // each with the balance available on the provisions drawn upon (V-02, BUD-293). Reads: nothing is written.

    public sealed record GetUnforeseenListQuery : IRequest<List<UnforeseenRequestDto>>;
    public sealed record GetUnforeseenByIdQuery(long Id) : IRequest<UnforeseenRequestDto?>;

    public sealed class UnforeseenReads(BudgetContext context)
    {
        public async Task<UnforeseenRequestDto> ToDtoAsync(long id, int? workId, string? workCode, long divisionOfficeId,
            string status, long holderOfficeId, long? currentProvisionId, decimal? amountCurrent,
            long? nextProvisionId, decimal? amountNext, CancellationToken ct)
        {
            decimal? Balance(long? provisionId) => provisionId is null
                ? null
                : context.ProvisionBalances.AsNoTracking()
                    .Where(v => v.ProvisionId == provisionId.Value).Select(v => (decimal?)v.Available).FirstOrDefault();
            await Task.CompletedTask;
            return new UnforeseenRequestDto
            {
                UnforeseenRequestId = id,
                WorkId = workId ?? 0,
                WorkCode = workCode,
                DivisionOfficeId = divisionOfficeId,
                Status = status,
                HolderOfficeId = holderOfficeId,
                CurrentProvisionId = currentProvisionId,
                AmountCurrent = amountCurrent,
                CurrentBalanceAvailable = Balance(currentProvisionId),
                NextProvisionId = nextProvisionId,
                AmountNext = amountNext,
                NextBalanceAvailable = Balance(nextProvisionId),
            };
        }
    }

    public sealed class GetUnforeseenListHandler(BudgetContext context, IBudgetActorSource actorSource, UnforeseenReads reads)
        : IRequestHandler<GetUnforeseenListQuery, List<UnforeseenRequestDto>>
    {
        public async Task<List<UnforeseenRequestDto>> Handle(GetUnforeseenListQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return [];
            var rows = await (
                from r in context.UnforeseenRequests.AsNoTracking()
                where r.HolderOfficeId == actor.OfficeId
                join w in context.Works.AsNoTracking() on r.WorkId equals w.Id into wj
                from w in wj.DefaultIfEmpty()
                orderby r.UnforeseenRequestId
                select new { r.UnforeseenRequestId, r.WorkId, w.WorkCode, r.DivisionOfficeId, r.Status, r.HolderOfficeId,
                             r.CurrentProvisionId, r.AmountCurrent, r.NextProvisionId, r.AmountNext }).ToListAsync(ct);

            var result = new List<UnforeseenRequestDto>();
            foreach (var r in rows)
                result.Add(await reads.ToDtoAsync(r.UnforeseenRequestId, r.WorkId, r.WorkCode, r.DivisionOfficeId, r.Status,
                    r.HolderOfficeId, r.CurrentProvisionId, r.AmountCurrent, r.NextProvisionId, r.AmountNext, ct));
            return result;
        }
    }

    public sealed class GetUnforeseenByIdHandler(BudgetContext context, UnforeseenReads reads)
        : IRequestHandler<GetUnforeseenByIdQuery, UnforeseenRequestDto?>
    {
        public async Task<UnforeseenRequestDto?> Handle(GetUnforeseenByIdQuery request, CancellationToken ct)
        {
            var r = await (
                from req in context.UnforeseenRequests.AsNoTracking()
                where req.UnforeseenRequestId == request.Id
                join w in context.Works.AsNoTracking() on req.WorkId equals w.Id into wj
                from w in wj.DefaultIfEmpty()
                select new { req.UnforeseenRequestId, req.WorkId, w.WorkCode, req.DivisionOfficeId, req.Status, req.HolderOfficeId,
                             req.CurrentProvisionId, req.AmountCurrent, req.NextProvisionId, req.AmountNext }).FirstOrDefaultAsync(ct);
            if (r is null)
                return null;
            return await reads.ToDtoAsync(r.UnforeseenRequestId, r.WorkId, r.WorkCode, r.DivisionOfficeId, r.Status,
                r.HolderOfficeId, r.CurrentProvisionId, r.AmountCurrent, r.NextProvisionId, r.AmountNext, ct);
        }
    }

    public sealed class UnforeseenReadEndpoints : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/unforeseen-requests",
                    async (IMediator m, CancellationToken ct) => Results.Ok(await m.Send(new GetUnforeseenListQuery(), ct)))
                .RequireAuthorization("BudgetRight:Budget.View").WithName("GetUnforeseenList").WithTags("UnforeseenWorks")
                .Produces<List<UnforeseenRequestDto>>(StatusCodes.Status200OK);

            // Read via POST: unforeseenRequestId travels in the body, not the URL (security requirement).
            app.MapPost("/api/unforeseen-requests/detail",
                    async (UnforeseenDetailRequestDto body, IMediator m, CancellationToken ct) =>
                    {
                        var dto = await m.Send(new GetUnforeseenByIdQuery(body.UnforeseenRequestId), ct);
                        return dto is null ? Results.NotFound() : Results.Ok(dto);
                    })
                .RequireAuthorization("BudgetRight:Budget.View").WithName("GetUnforeseenById").WithTags("UnforeseenWorks")
                .Produces<UnforeseenRequestDto>(StatusCodes.Status200OK).Produces(StatusCodes.Status404NotFound)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
