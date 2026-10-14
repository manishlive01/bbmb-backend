using System.Collections.Concurrent;
using Auth.Authorization.Requirements;
using BBMB.BudgetService.Features.BudgetProcess.Cycles.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Offices;
using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using BBMB.BudgetService.Shared.Services.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules
{
    /// <summary>
    /// Design §6.2, §8.1, §4.9: the single path by which a proposal moves. Order of checks:
    /// state model present (else M-19) → actor (M-20) → visible (404) → holder (409 M-19; not for RECALL) →
    /// cycle not frozen/closed (M-17) → target resolved and present in T-16 (M-19) → the transition's right
    /// (M-20) → remark where T-16 requires it (M-12) → hooks → one T-33 row + the T-29 update in one action
    /// scope; the lease is cleared; notification after commit.
    /// </summary>
    public sealed class ProposalTransitionService(
        BudgetContext context,
        IBudgetActorSource actorSource,
        IBudgetActionScope scope,
        IBudgetOfficeScope officeScope,
        IAuthorizationService authorization,
        IHttpContextAccessor httpContextAccessor,
        IBudgetNotificationGateway notifications,
        ILogger<ProposalTransitionService> logger) : IProposalTransitionService
    {
        private static readonly ConcurrentDictionary<string, bool> StateModelAvailable = new(StringComparer.Ordinal);

        public async Task<bool> IsStateModelAvailableAsync(CancellationToken ct = default)
        {
            var key = context.Database.GetConnectionString() ?? string.Empty;
            if (StateModelAvailable.ContainsKey(key))
                return true;
            var present = await context.Database
                .SqlQueryRaw<bool>("SELECT to_regclass('budget.state_transition') IS NOT NULL AS \"Value\"")
                .SingleAsync(ct);
            if (present)
                StateModelAvailable.TryAdd(key, true); // the table is never dropped (fn_forbid_change)
            return present;
        }

        public async Task<Response> MoveAsync(
            long proposalId, string action, string? remark, CancellationToken ct = default, TransitionHooks? hooks = null)
        {
            hooks ??= TransitionHooks.None;

            // §11.4: before BudgetP1StateTransitionSeed no move is possible; checked before the proposal is read.
            if (!await IsStateModelAvailableAsync(ct))
                return BudgetResponses.Fail<ProposalStateResponseDto>(BudgetMessages.M19);

            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<ProposalStateResponseDto>(BudgetMessages.M20);

            var proposal = await context.BudgetProposals.FirstOrDefaultAsync(p => p.ProposalId == proposalId, ct);
            if (proposal is null)
                return BudgetResponses.Fail<ProposalStateResponseDto>(BudgetMessages.NotFound);

            var visible = await officeScope.GetVisibleOfficeIdsAsync(actor.OfficeId, ct);
            if (!visible.Contains(proposal.OriginOfficeId) && !visible.Contains(proposal.HolderOfficeId))
                return BudgetResponses.Fail<ProposalStateResponseDto>(BudgetMessages.NotFound);

            if (action != ProposalAction.Recall && proposal.HolderOfficeId != actor.OfficeId)
                return BudgetResponses.Fail<ProposalStateResponseDto>(BudgetMessages.M19);

            var cycleStatus = await context.BudgetCycles.AsNoTracking()
                .Where(c => c.CycleId == proposal.CycleId).Select(c => c.Status).SingleAsync(ct);
            if (cycleStatus != CycleStatus.Open && action != ProposalAction.Freeze)
                return BudgetResponses.Fail<ProposalStateResponseDto>(BudgetMessages.M17);

            var resolved = await ResolveAsync(proposal, action, actor.OfficeId, ct);
            if (resolved is null)
                return BudgetResponses.Fail<ProposalStateResponseDto>(BudgetMessages.M19);

            var transition = await context.StateTransitions.AsNoTracking().FirstOrDefaultAsync(t =>
                t.FromState == resolved.FromState && t.ActionCode == resolved.ActionCode && t.ToState == resolved.ToState, ct);
            if (transition is null)
                return BudgetResponses.Fail<ProposalStateResponseDto>(BudgetMessages.M19);

            var right = TransitionResolver.RightFor(action, proposal.StateCode, proposal.Category);
            var user = httpContextAccessor.HttpContext?.User ?? new System.Security.Claims.ClaimsPrincipal();
            if (!(await authorization.AuthorizeAsync(user, null, BudgetRightRequirement.Policy(right))).Succeeded)
                return BudgetResponses.Fail<ProposalStateResponseDto>(BudgetMessages.M20);

            if (transition.RemarkRequired && string.IsNullOrWhiteSpace(remark))
                return BudgetResponses.Fail<ProposalStateResponseDto>(BudgetMessages.M12);

            ProposalMovement movement;
            await using (var tx = await scope.BeginAsync(action, ct: ct))
            {
                // The hook's refusal is returned as-is so a Response<RuleFailuresDto> keeps Data.Failures[]
                // (design §4.9, BUD-096, BUD-100). Disposing the scope uncommitted rolls back anything it read or locked.
                if (hooks.BeforeMove is not null && await hooks.BeforeMove(proposal, ct) is { } refusal)
                    return refusal;

                movement = new ProposalMovement
                {
                    ProposalId = proposal.ProposalId,
                    FromState = resolved.FromState,
                    ActionCode = resolved.ActionCode,
                    ToState = resolved.ToState,
                    FromOfficeId = actor.OfficeId,
                    ToOfficeId = resolved.ToOfficeId,
                    Remark = string.IsNullOrWhiteSpace(remark) ? null : remark.Trim(),
                    RoleCode = actor.RoleCode,
                    OnBehalfOfPost = actor.OnBehalfOfPost,
                    ClientIp = actor.ClientIp,
                };
                context.ProposalMovements.Add(movement);

                proposal.StateCode = resolved.ToState;
                proposal.HolderOfficeId = resolved.ToOfficeId;
                if (action == ProposalAction.Submit && proposal.FirstSubmittedAt is null)
                    proposal.FirstSubmittedAt = DateTimeOffset.UtcNow;
                // The edit lease is cleared on every transition (design §6.2 rules list).
                proposal.EditLeaseBy = null;
                proposal.EditLeaseExpiresAt = null;

                await context.SaveChangesAsync(ct);
                if (hooks.AfterMove is not null)
                {
                    await hooks.AfterMove(proposal, movement, ct);
                    await context.SaveChangesAsync(ct);
                }
                await tx.CommitAsync(ct);
            }

            try
            {
                await notifications.NotifyMovementAsync(proposal.ProposalId, movement.MovementId, ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Movement notification failed for proposal {ProposalId}, movement {MovementId}",
                    proposal.ProposalId, movement.MovementId);
            }

            return new Response<ProposalStateResponseDto>
            {
                IsSucced = true,
                Message = "Proposal moved",
                Data = await StateOfAsync(proposal.ProposalId, ct),
            };
        }

        /// <summary>The proposal's state and holder for a response (design §8.1).</summary>
        public async Task<ProposalStateResponseDto> StateOfAsync(long proposalId, CancellationToken ct)
        {
            var row = await (
                from p in context.BudgetProposals.AsNoTracking()
                join s in context.ProposalStates on p.StateCode equals s.StateCode
                where p.ProposalId == proposalId
                select new { p.ProposalId, p.StateCode, s.StateName, p.HolderOfficeId }).SingleAsync(ct);
            var names = await BudgetOfficeNames.ResolveAsync(context, [row.HolderOfficeId], ct);
            return new ProposalStateResponseDto
            {
                ProposalId = row.ProposalId,
                StateCode = row.StateCode,
                StateName = row.StateName,
                HolderOfficeId = row.HolderOfficeId,
                HolderOfficeName = names.GetValueOrDefault(row.HolderOfficeId),
            };
        }

        private async Task<ResolvedTransition?> ResolveAsync(BudgetProposal proposal, string action, long actorOfficeId, CancellationToken ct)
        {
            var history = await context.ProposalMovements.AsNoTracking()
                .Where(m => m.ProposalId == proposal.ProposalId)
                .OrderBy(m => m.MovementId)
                .Select(m => new TransitionHistoryRow(m.FromState, m.ActionCode, m.ToState, m.FromOfficeId, m.ToOfficeId))
                .ToListAsync(ct);

            var officeIds = history.SelectMany(h => new[] { h.FromOfficeId, h.ToOfficeId })
                .Append(proposal.OriginOfficeId).Append(proposal.HolderOfficeId).Distinct().ToList();
            var offices = await context.BudgetOffices.AsNoTracking()
                .Where(o => officeIds.Contains(o.OfficeId) || (o.IsActive &&
                            (o.OfficeLevel == OfficeLevel.Finance || o.OfficeLevel == OfficeLevel.Secretariat)))
                .Select(o => new { o.OfficeId, o.OfficeLevel, o.ParentOfficeId, o.IsActive })
                .ToListAsync(ct);

            var origin = offices.Single(o => o.OfficeId == proposal.OriginOfficeId);
            var holder = offices.Single(o => o.OfficeId == proposal.HolderOfficeId);
            var finance = offices.Where(o => o.IsActive && o.OfficeLevel == OfficeLevel.Finance).Select(o => (long?)o.OfficeId).ToList();
            var secretariat = offices.Where(o => o.IsActive && o.OfficeLevel == OfficeLevel.Secretariat).Select(o => (long?)o.OfficeId).ToList();

            return TransitionResolver.Resolve(
                action,
                new TransitionProposal(proposal.StateCode, proposal.Category, origin.OfficeId, origin.OfficeLevel,
                    origin.ParentOfficeId, holder.OfficeId, holder.ParentOfficeId),
                history,
                actorOfficeId,
                new TransitionOffices(
                    finance.Count == 1 ? finance[0] : null,
                    secretariat.Count == 1 ? secretariat[0] : null,
                    offices.ToDictionary(o => o.OfficeId, o => o.OfficeLevel)));
        }
    }
}
