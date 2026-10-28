using BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Sources
{
    // Design §8.7 F-21 surrender lines: add, update, remove. Each operates only while the entry is OPEN (DB-24),
    // keeps the cumulative within the additional amount (VR-08, M-18) and returns the amount still to surrender
    // (BUD-200). The DB triggers (fn_reappropriation_source_rules, fn_reappropriation_balance) enforce the same.

    public sealed record AddSourceDto(long ProvisionId, decimal Amount);
    public sealed record UpdateSourceDto(decimal Amount);
    public sealed record ReappropriationBalanceDto(decimal Additional, decimal Surrendered, decimal ToSurrender);

    public sealed record AddSourceCommand(long ReappropriationId, AddSourceDto Dto) : IRequest<Response>;
    public sealed record UpdateSourceCommand(long ReappropriationId, long SourceId, UpdateSourceDto Dto) : IRequest<Response>;
    public sealed record RemoveSourceCommand(long ReappropriationId, long SourceId) : IRequest<Response>;

    public abstract class SourceHandlerBase(BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, ReappropriationRules rules)
    {
        protected BudgetContext Context { get; } = context;
        protected IBudgetActorSource ActorSource { get; } = actorSource;
        protected IBudgetActionScope Scope { get; } = scope;
        protected ReappropriationRules Rules { get; } = rules;

        protected async Task<(Reappropriation? Entry, string? Fail)> OpenEntryAsync(long id, CancellationToken ct)
        {
            if (await ActorSource.GetAsync(ct) is null)
                return (null, BudgetMessages.M20);
            var entry = await Context.Reappropriations.FirstOrDefaultAsync(r => r.ReappropriationId == id, ct);
            if (entry is null)
                return (null, BudgetMessages.NotFound);
            if (entry.Status != ReappropriationStatus.Open)
                return (null, BudgetMessages.M19); // edits only while OPEN (DB-24)
            return (entry, null);
        }

        protected Response<ReappropriationBalanceDto> Balance(Reappropriation entry, decimal surrendered) =>
            new()
            {
                IsSucced = true,
                Message = "Surrender recorded",
                Data = new ReappropriationBalanceDto(entry.AdditionalAmount, surrendered,
                    ReappropriationRules.ToSurrender(entry.AdditionalAmount, surrendered)),
            };
    }

    public sealed class AddSourceHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, ReappropriationRules rules)
        : SourceHandlerBase(context, actorSource, scope, rules), IRequestHandler<AddSourceCommand, Response>
    {
        public async Task<Response> Handle(AddSourceCommand request, CancellationToken ct)
        {
            var (entry, fail) = await OpenEntryAsync(request.ReappropriationId, ct);
            if (entry is null)
                return BudgetResponses.Fail(fail!);
            var dto = request.Dto;
            if (dto.Amount <= 0)
                return BudgetResponses.Fail(BudgetMessages.M22);
            // Not the destination provision (DB-24); no eligibility filter otherwise (BUD-198).
            if (entry.DestinationProvisionId == dto.ProvisionId)
                return BudgetResponses.Fail(BudgetMessages.M19);
            if (!await Context.Provisions.AsNoTracking().AnyAsync(p => p.ProvisionId == dto.ProvisionId && p.SupersededByProvisionId == null, ct))
                return BudgetResponses.Fail(BudgetMessages.M19);
            if (await Context.ReappropriationSources.AsNoTracking()
                    .AnyAsync(s => s.ReappropriationId == entry.ReappropriationId && s.ProvisionId == dto.ProvisionId && !s.IsRemoved, ct))
                return BudgetResponses.Fail(BudgetMessages.M19); // one live source per provision (UK)

            var cumulative = await Rules.SurrenderedAsync(entry.ReappropriationId, ct: ct) + dto.Amount;
            if (!ReappropriationRules.WithinAdditional(entry.AdditionalAmount, cumulative))
                return BudgetResponses.Fail(BudgetMessages.M18, new ReappropriationBalanceDto(
                    entry.AdditionalAmount, cumulative - dto.Amount, ReappropriationRules.ToSurrender(entry.AdditionalAmount, cumulative - dto.Amount)));

            await using var tx = await Scope.BeginAsync("REAPPROP_ADD_SOURCE", ct: ct);
            Context.ReappropriationSources.Add(new ReappropriationSource
            {
                ReappropriationId = entry.ReappropriationId, ProvisionId = dto.ProvisionId, Amount = dto.Amount,
            });
            await Context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return Balance(entry, cumulative);
        }
    }

    public sealed class UpdateSourceHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, ReappropriationRules rules)
        : SourceHandlerBase(context, actorSource, scope, rules), IRequestHandler<UpdateSourceCommand, Response>
    {
        public async Task<Response> Handle(UpdateSourceCommand request, CancellationToken ct)
        {
            var (entry, fail) = await OpenEntryAsync(request.ReappropriationId, ct);
            if (entry is null)
                return BudgetResponses.Fail(fail!);
            if (request.Dto.Amount <= 0)
                return BudgetResponses.Fail(BudgetMessages.M22);
            var source = await Context.ReappropriationSources.FirstOrDefaultAsync(s =>
                s.ReappropriationSourceId == request.SourceId && s.ReappropriationId == entry.ReappropriationId && !s.IsRemoved, ct);
            if (source is null)
                return BudgetResponses.Fail(BudgetMessages.NotFound);

            var cumulative = await Rules.SurrenderedAsync(entry.ReappropriationId, excludingSourceId: source.ReappropriationSourceId, ct: ct) + request.Dto.Amount;
            if (!ReappropriationRules.WithinAdditional(entry.AdditionalAmount, cumulative))
                return BudgetResponses.Fail(BudgetMessages.M18, new ReappropriationBalanceDto(
                    entry.AdditionalAmount, cumulative - request.Dto.Amount, ReappropriationRules.ToSurrender(entry.AdditionalAmount, cumulative - request.Dto.Amount)));

            await using var tx = await Scope.BeginAsync("REAPPROP_UPDATE_SOURCE", ct: ct);
            source.Amount = request.Dto.Amount;
            await Context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return Balance(entry, cumulative);
        }
    }

    public sealed class RemoveSourceHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, ReappropriationRules rules)
        : SourceHandlerBase(context, actorSource, scope, rules), IRequestHandler<RemoveSourceCommand, Response>
    {
        public async Task<Response> Handle(RemoveSourceCommand request, CancellationToken ct)
        {
            var (entry, fail) = await OpenEntryAsync(request.ReappropriationId, ct);
            if (entry is null)
                return BudgetResponses.Fail(fail!);
            var source = await Context.ReappropriationSources.FirstOrDefaultAsync(s =>
                s.ReappropriationSourceId == request.SourceId && s.ReappropriationId == entry.ReappropriationId && !s.IsRemoved, ct);
            if (source is null)
                return BudgetResponses.Fail(BudgetMessages.NotFound);

            await using var tx = await Scope.BeginAsync("REAPPROP_REMOVE_SOURCE", ct: ct);
            source.IsRemoved = true; // one-way (§5.5 row c); excluded from totals thereafter
            await Context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            var surrendered = await Rules.SurrenderedAsync(entry.ReappropriationId, ct: ct);
            return Balance(entry, surrendered);
        }
    }

    public sealed class ReappropriationSourceEndpoints : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/reappropriations/{id:long}/sources",
                    async (long id, AddSourceDto dto, IMediator m, CancellationToken ct) =>
                        BudgetResults.Ok(await m.Send(new AddSourceCommand(id, dto), ct)))
                .RequireAuthorization("BudgetRight:F21.Enter").WithName("AddReappropriationSource").WithTags("Reappropriations")
                .Produces<Response<ReappropriationBalanceDto>>(StatusCodes.Status200OK)
                .Produces<Response<ReappropriationBalanceDto>>(StatusCodes.Status409Conflict);

            app.MapPut("/api/reappropriations/{id:long}/sources/{sourceId:long}",
                    async (long id, long sourceId, UpdateSourceDto dto, IMediator m, CancellationToken ct) =>
                        BudgetResults.Ok(await m.Send(new UpdateSourceCommand(id, sourceId, dto), ct)))
                .RequireAuthorization("BudgetRight:F21.Enter").WithName("UpdateReappropriationSource").WithTags("Reappropriations")
                .Produces<Response<ReappropriationBalanceDto>>(StatusCodes.Status200OK)
                .Produces<Response<ReappropriationBalanceDto>>(StatusCodes.Status409Conflict);

            app.MapPost("/api/reappropriations/{id:long}/sources/{sourceId:long}/remove",
                    async (long id, long sourceId, IMediator m, CancellationToken ct) =>
                        BudgetResults.Ok(await m.Send(new RemoveSourceCommand(id, sourceId), ct)))
                .RequireAuthorization("BudgetRight:F21.Enter").WithName("RemoveReappropriationSource").WithTags("Reappropriations")
                .Produces<Response<ReappropriationBalanceDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status409Conflict);
        }
    }
}
