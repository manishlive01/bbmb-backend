using BBMB.BudgetService.Features.BudgetProcess.Settings.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.Settings.CreateDelegationLimit
{
    /// <summary>
    /// Appends one T-19 row (no PUT or DELETE exists; DB-27 refuses UPDATE and DELETE). A row for the same level
    /// and date is refused M-19 (the OI-13 uniqueness choice, as T-17); the UK maps to M-19 under concurrency.
    /// A change of limit is a new row with a later effectiveFrom, so the limit in force on any past date stays
    /// reconstructable.
    /// </summary>
    public sealed class CreateDelegationLimitHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope)
        : IRequestHandler<CreateDelegationLimitCommand, Response<long>>
    {
        public const string ActionCode = "DELEGATION_LIMIT_CREATE";
        public async Task<Response<long>> Handle(CreateDelegationLimitCommand request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail<long>(BudgetMessages.M20);
            var dto = request.Dto;
            if (await context.DelegationLimits.AsNoTracking()
                    .AnyAsync(d => d.AuthorityLevel == dto.AuthorityLevel && d.EffectiveFrom == dto.EffectiveFrom, ct))
                return BudgetResponses.Fail<long>(BudgetMessages.M19);
            var row = new DelegationLimit
            {
                AuthorityLevel = dto.AuthorityLevel, MaxAmount = dto.MaxAmount, EffectiveFrom = dto.EffectiveFrom,
                SourceRef = dto.SourceRef.Trim(),
            };
            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            context.DelegationLimits.Add(row);
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new Response<long> { IsSucced = true, Message = "Delegation limit recorded", Data = row.DelegationLimitId };
        }
    }
}
