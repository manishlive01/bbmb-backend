using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.Remove
{
    /// <summary>
    /// F-17 Remove (design §8.4; BUD-170, DB-02). Removal is by marking, never by deletion. Order: actor (M-20) → absent
    /// NOT_FOUND → already removed M-19 → a non-deleted head-wise demand line of the office's demands uses the head M-15
    /// (the provision arm is added in P4 with T-41) → <c>removed_at</c> in one REMOVE_HEAD scope; the trigger takes the
    /// time and the officer from the actor context. DB-02 is the backstop.
    /// </summary>
    public sealed class RemoveHeadAssignmentHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope,
        TimeProvider? clock = null)
        : IRequestHandler<RemoveHeadAssignmentCommand, Response>
    {
        public const string ActionCode = "REMOVE_HEAD";

        public async Task<Response> Handle(RemoveHeadAssignmentCommand request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail(BudgetMessages.M20);
            var row = await context.AccountHeadAssignments.FirstOrDefaultAsync(a => a.AssignmentId == request.AssignmentId, ct);
            if (row is null)
                return BudgetResponses.Fail(BudgetMessages.NotFound);
            if (row.RemovedAt is not null)
                return BudgetResponses.Fail(BudgetMessages.M19);
            var inUse = await context.DemandLines.AsNoTracking()
                .Join(context.BudgetProposals, l => l.ProposalId, p => p.ProposalId, (l, p) => new { l, p.OriginOfficeId })
                .AnyAsync(x => x.OriginOfficeId == row.OfficeId && x.l.AccountCodeId == row.AccountCodeId &&
                               x.l.LineType == DemandLineType.Head && !x.l.IsDeleted, ct);
            if (inUse)
                return BudgetResponses.Fail(BudgetMessages.M15);
            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            row.RemovedAt = (clock ?? TimeProvider.System).GetUtcNow(); // the trigger re-stamps it and sets removed_by
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new Response { IsSucced = true, Message = "Head removed from the office" };
        }
    }
}
