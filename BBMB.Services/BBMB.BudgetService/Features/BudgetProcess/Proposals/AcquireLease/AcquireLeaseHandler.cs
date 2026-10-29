using BBMB.BudgetService.Authorization.Requirements;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.AcquireLease
{
    /// <summary>
    /// Design §4.10, §8.3. Out of scope → NOT_FOUND. Not the holder office, a state with no write, or no write
    /// right for the acting role → 200 granted:false, readOnly:true (a reader cannot block a writer, BUD-015).
    /// Another officer's live lease → M-23. Otherwise the lease is taken or renewed (action LEASE).
    /// </summary>
    public sealed class AcquireLeaseHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, IBudgetOfficeScope officeScope,
        IAuthorizationService authorization, IHttpContextAccessor httpContextAccessor, IConfiguration configuration,
        TimeProvider? clock = null)
        : IRequestHandler<AcquireLeaseCommand, Response<LeaseResponseDto>>
    {
        public const string ActionCode = "LEASE";

        public async Task<Response<LeaseResponseDto>> Handle(AcquireLeaseCommand request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<LeaseResponseDto>(BudgetMessages.M20);
            var p = await ProposalReads.FindVisibleAsync(context, officeScope, actor, request.ProposalId, true, ct);
            if (p is null)
                return BudgetResponses.Fail<LeaseResponseDto>(BudgetMessages.NotFound);

            var now = (clock ?? TimeProvider.System).GetUtcNow();
            var live = ProposalLease.IsLive(p, now);
            LeaseResponseDto ReadOnly() => new(false, true, live ? p.EditLeaseBy : null, live ? p.EditLeaseExpiresAt : null);

            var right = ProposalLease.WriteRightFor(p.StateCode, p.Category);
            var user = httpContextAccessor.HttpContext?.User ?? new System.Security.Claims.ClaimsPrincipal();
            if (p.HolderOfficeId != actor.OfficeId || right is null ||
                !(await authorization.AuthorizeAsync(user, null, BudgetRightRequirement.Policy(right))).Succeeded)
                return new Response<LeaseResponseDto> { IsSucced = true, Message = "Opened read-only", Data = ReadOnly() };

            if (ProposalLease.IsHeldByOther(p, actor.EmployeeId, now))
                return BudgetResponses.Fail(BudgetMessages.M23, ReadOnly());

            p.EditLeaseBy = actor.EmployeeId;
            p.EditLeaseExpiresAt = now + ProposalLease.Length(configuration);
            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new Response<LeaseResponseDto>
            {
                IsSucced = true, Message = "Lease granted",
                Data = new LeaseResponseDto(true, false, p.EditLeaseBy, p.EditLeaseExpiresAt),
            };
        }
    }
}
