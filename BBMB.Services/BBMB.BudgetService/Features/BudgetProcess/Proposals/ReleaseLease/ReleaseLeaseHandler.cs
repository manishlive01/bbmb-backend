using BBMB.BudgetService.Features.BudgetProcess.Proposals.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.ReleaseLease
{
    /// <summary>
    /// Releases the actor's own lease (action LEASE_RELEASE). Another officer's live lease is not released
    /// (M-23); with no live lease, nothing is written.
    /// </summary>
    public sealed class ReleaseLeaseHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, IBudgetOfficeScope officeScope,
        TimeProvider? clock = null)
        : IRequestHandler<ReleaseLeaseCommand, Response<LeaseResponseDto>>
    {
        public const string ActionCode = "LEASE_RELEASE";

        public async Task<Response<LeaseResponseDto>> Handle(ReleaseLeaseCommand request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<LeaseResponseDto>(BudgetMessages.M20);
            var p = await ProposalReads.FindVisibleAsync(context, officeScope, actor, request.ProposalId, true, ct);
            if (p is null)
                return BudgetResponses.Fail<LeaseResponseDto>(BudgetMessages.NotFound);

            var now = (clock ?? TimeProvider.System).GetUtcNow();
            if (ProposalLease.IsHeldByOther(p, actor.EmployeeId, now))
                return BudgetResponses.Fail(BudgetMessages.M23, new LeaseResponseDto(false, true, p.EditLeaseBy, p.EditLeaseExpiresAt));
            if (p.EditLeaseBy == actor.EmployeeId)
            {
                p.EditLeaseBy = null;
                p.EditLeaseExpiresAt = null;
                await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
                await context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            return new Response<LeaseResponseDto> { IsSucced = true, Message = "Lease released", Data = new LeaseResponseDto(false, false, null, null) };
        }
    }
}
