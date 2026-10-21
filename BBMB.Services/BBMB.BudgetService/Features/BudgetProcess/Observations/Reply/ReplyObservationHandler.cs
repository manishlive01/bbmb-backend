using BBMB.BudgetService.Features.BudgetProcess.Cycles.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.Observations.Reply
{
    /// <summary>
    /// Design §8.3 Reply (BUD-121): the origin office replies at S-04 to an observation that was returned to it.
    /// Order: actor (M-20) → visible (NOT_FOUND) → origin office (M-20) → S-04 held by the origin (M-19) → cycle
    /// OPEN (M-17) → lease (M-23) → the observation is of this proposal (NOT_FOUND) → it was returned
    /// (<c>returned_by_movement_id</c> set) and has no reply yet, else M-19 (DB-15: written once). replied_at /
    /// replied_by are set from the actor context by the protect trigger. Right Proposal.Reply (endpoint).
    /// </summary>
    public sealed class ReplyObservationHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, IBudgetOfficeScope officeScope,
        IConfiguration configuration, TimeProvider? clock = null)
        : IRequestHandler<ReplyObservationCommand, Response>
    {
        public const string ActionCode = "OBSERVATION_REPLY";

        public async Task<Response> Handle(ReplyObservationCommand request, CancellationToken ct)
        {
            var now = (clock ?? TimeProvider.System).GetUtcNow();
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail(BudgetMessages.M20);
            var p = await ProposalReads.FindVisibleAsync(context, officeScope, actor, request.ProposalId, true, ct);
            if (p is null)
                return BudgetResponses.Fail(BudgetMessages.NotFound);
            if (p.OriginOfficeId != actor.OfficeId)
                return BudgetResponses.Fail(BudgetMessages.M20);
            if (p.StateCode != ProposalStateCode.S04 || p.HolderOfficeId != p.OriginOfficeId)
                return BudgetResponses.Fail(BudgetMessages.M19);
            var cycleStatus = await context.BudgetCycles.AsNoTracking()
                .Where(c => c.CycleId == p.CycleId).Select(c => c.Status).SingleAsync(ct);
            if (cycleStatus != CycleStatus.Open)
                return BudgetResponses.Fail(BudgetMessages.M17);
            if (ProposalLease.IsHeldByOther(p, actor.EmployeeId, now))
                return BudgetResponses.Fail(BudgetMessages.M23);
            var o = await context.Observations
                .FirstOrDefaultAsync(x => x.ObservationId == request.ObservationId && x.ProposalId == p.ProposalId, ct);
            if (o is null)
                return BudgetResponses.Fail(BudgetMessages.NotFound);
            if (o.ReturnedByMovementId is null || o.ReplyText is not null)
                return BudgetResponses.Fail(BudgetMessages.M19);
            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            o.ReplyText = request.Dto.ReplyText.Trim();
            p.EditLeaseBy = actor.EmployeeId;
            p.EditLeaseExpiresAt = now + ProposalLease.Length(configuration);
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new Response { IsSucced = true, Message = "Reply recorded" };
        }
    }
}
