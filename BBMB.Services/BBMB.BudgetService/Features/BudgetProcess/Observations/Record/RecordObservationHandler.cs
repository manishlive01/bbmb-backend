using BBMB.BudgetService.Authorization.Requirements;
using BBMB.BudgetService.Features.BudgetProcess.Cycles.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Observations.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.Observations.Record
{
    /// <summary>
    /// Design §8.3 Record (BUD-120, BUD-121, BUD-128). Order: actor (M-20) → visible (NOT_FOUND) → the actor's
    /// office holds the proposal (M-20, §4.9) → state S-03 / S-06 / S-08 (right Review.Examine) or S-10 (right
    /// F10.Act for WORKS, F13.Act head-wise) else M-19 → right (M-20) → cycle OPEN (M-17) → another officer's
    /// live lease (M-23) → the line, where named, is of this proposal (NOT_FOUND; DB-04 backstop). The row is
    /// recorded at the actor's level; its officer is the [C] columns (fn_require_actor). Fixed once written (DB-15).
    /// </summary>
    public sealed class RecordObservationHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, IBudgetOfficeScope officeScope,
        IAuthorizationService authorization, IHttpContextAccessor httpContextAccessor, IConfiguration configuration,
        TimeProvider? clock = null)
        : IRequestHandler<RecordObservationCommand, Response<long>>
    {
        public const string ActionCode = "OBSERVATION_RECORD";
        private static readonly string[] ReviewerStates = [ProposalStateCode.S03, ProposalStateCode.S06, ProposalStateCode.S08];

        public async Task<Response<long>> Handle(RecordObservationCommand request, CancellationToken ct)
        {
            var now = (clock ?? TimeProvider.System).GetUtcNow();
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<long>(BudgetMessages.M20);
            var p = await ProposalReads.FindVisibleAsync(context, officeScope, actor, request.ProposalId, true, ct);
            if (p is null)
                return BudgetResponses.Fail<long>(BudgetMessages.NotFound);
            if (p.HolderOfficeId != actor.OfficeId)
                return BudgetResponses.Fail<long>(BudgetMessages.M20);
            string right;
            if (ReviewerStates.Contains(p.StateCode)) right = "Review.Examine";
            else if (p.StateCode == ProposalStateCode.S10) right = p.Category == ProposalCategory.Works ? "F10.Act" : "F13.Act";
            else return BudgetResponses.Fail<long>(BudgetMessages.M19);
            var user = httpContextAccessor.HttpContext?.User ?? new System.Security.Claims.ClaimsPrincipal();
            if (!(await authorization.AuthorizeAsync(user, null, BudgetRightRequirement.Policy(right))).Succeeded)
                return BudgetResponses.Fail<long>(BudgetMessages.M20);
            var cycleStatus = await context.BudgetCycles.AsNoTracking()
                .Where(c => c.CycleId == p.CycleId).Select(c => c.Status).SingleAsync(ct);
            if (cycleStatus != CycleStatus.Open)
                return BudgetResponses.Fail<long>(BudgetMessages.M17);
            if (ProposalLease.IsHeldByOther(p, actor.EmployeeId, now))
                return BudgetResponses.Fail<long>(BudgetMessages.M23);
            if (request.Dto.DemandLineId is { } lineId &&
                !await context.DemandLines.AsNoTracking().AnyAsync(l => l.DemandLineId == lineId && l.ProposalId == p.ProposalId, ct))
                return BudgetResponses.Fail<long>(BudgetMessages.NotFound);

            var row = new Observation
            {
                ProposalId = p.ProposalId, DemandLineId = request.Dto.DemandLineId, LevelOfficeId = actor.OfficeId,
                ObservationText = request.Dto.ObservationText.Trim(),
            };
            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            context.Observations.Add(row);
            p.EditLeaseBy = actor.EmployeeId;
            p.EditLeaseExpiresAt = now + ProposalLease.Length(configuration);
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new Response<long> { IsSucced = true, Message = "Observation recorded", Data = row.ObservationId };
        }
    }
}
