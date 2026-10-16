using BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Abandon
{
    /// <summary>Design §8.7 abandon (F-21, BUD-206): an OPEN entry becomes ABANDONED and writes no movement.</summary>
    public sealed record AbandonReappropriationCommand(long Id) : IRequest<Response>;

    public sealed class AbandonReappropriationHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope)
        : IRequestHandler<AbandonReappropriationCommand, Response>
    {
        public async Task<Response> Handle(AbandonReappropriationCommand request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail(BudgetMessages.M20);
            var entry = await context.Reappropriations.FirstOrDefaultAsync(r => r.ReappropriationId == request.Id, ct);
            if (entry is null)
                return BudgetResponses.Fail(BudgetMessages.NotFound);
            if (entry.Status != ReappropriationStatus.Open)
                return BudgetResponses.Fail(BudgetMessages.M19); // moves from OPEN only (DB-24)

            await using var tx = await scope.BeginAsync("REAPPROP_ABANDON", ct: ct);
            entry.Status = ReappropriationStatus.Abandoned;
            entry.ClosedAt = DateTimeOffset.UtcNow;
            entry.ClosedBy = actor.EmployeeId;
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new Response { IsSucced = true, Message = "Re-appropriation abandoned" };
        }
    }

    public sealed class AbandonReappropriationEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/reappropriations/{id:long}/abandon",
                    async (long id, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new AbandonReappropriationCommand(id), ct)))
                .RequireAuthorization("BudgetRight:F21.Enter")
                .WithName("AbandonReappropriation")
                .WithTags("Reappropriations")
                .Produces<Response>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status409Conflict);
        }
    }
}
