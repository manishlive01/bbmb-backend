using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.MarkDeleted
{
    /// <summary>
    /// Design §8.3 MarkDeleted (BUD-319, BUD-320): the origin office at S-01 / S-04 marks <c>is_deleted</c>; the
    /// row, its T-31 versions and its observations stay (DB-13: no DELETE reaches the table), and the work and
    /// any work code are untouched. <c>deleted_at</c> / <c>deleted_by</c> are set by <c>fn_demand_line_rules</c>
    /// from the actor context, never from C#. A line already marked is answered as done (idempotent DELETE).
    /// </summary>
    public sealed class MarkDemandLineDeletedHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, IBudgetOfficeScope officeScope,
        IAuthorizationService authorization, IHttpContextAccessor httpContextAccessor, IConfiguration configuration,
        TimeProvider? clock = null)
        : IRequestHandler<MarkDemandLineDeletedCommand, Response>
    {
        public async Task<Response> Handle(MarkDemandLineDeletedCommand request, CancellationToken ct)
        {
            var now = (clock ?? TimeProvider.System).GetUtcNow();
            var target = await DemandLineWrites.ResolveAsync(
                context, actorSource, officeScope, authorization, httpContextAccessor, request.ProposalId, now, ct);
            if (target.Refusal is { } refusal)
                return refusal;
            var (actor, p) = (target.Actor!, target.Proposal!);
            await using var tx = await scope.BeginAsync(DemandLineWrites.DeleteAction, ct: ct);
            var line = await context.DemandLines
                .FirstOrDefaultAsync(l => l.DemandLineId == request.DemandLineId && l.ProposalId == p.ProposalId, ct);
            if (line is null)
                return BudgetResponses.Fail(BudgetMessages.NotFound);
            if (!line.IsDeleted)
            {
                line.IsDeleted = true;
                DemandLineWrites.RenewLease(p, actor, now, ProposalLease.Length(configuration));
                await context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            return new Response { IsSucced = true, Message = "Demand line marked deleted" };
        }
    }
}
