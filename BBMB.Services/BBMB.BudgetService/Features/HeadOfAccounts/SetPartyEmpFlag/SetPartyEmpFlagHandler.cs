using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.HeadOfAccounts.SetPartyEmpFlag
{
    /// <summary>
    /// BUD-268 (design §8.4): the ACC-009 flag is altered under the IT Cell only. Order: actor (M-20) → the actor's office
    /// is the IT Cell row (T-51 <c>actor_office_id</c> is then that office) else M-20 → code absent NOT_FOUND → unchanged
    /// value: success without a write → in one F01_PARTY_FLAG scope the new value. trg_head_master_protect refuses the
    /// change from any other office (DB-03).
    /// </summary>
    public sealed class SetPartyEmpFlagHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope)
        : IRequestHandler<SetPartyEmpFlagCommand, Response>
    {
        public const string ActionCode = "F01_PARTY_FLAG";

        public async Task<Response> Handle(SetPartyEmpFlagCommand request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null || actor.OfficeLevel != OfficeLevel.ItCell)
                return BudgetResponses.Fail(BudgetMessages.M20);
            var code = await context.HeadOfAccounts.FirstOrDefaultAsync(h => h.AccountCodeId == request.AccountCodeId, ct);
            if (code is null)
                return BudgetResponses.Fail(BudgetMessages.NotFound);
            if (code.PartyEmpRequired == request.Dto.Value)
                return new Response { IsSucced = true, Message = "Flag unchanged" };
            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            code.PartyEmpRequired = request.Dto.Value;
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new Response { IsSucced = true, Message = $"Party/employee flag of {code.CompleteCode} set to {request.Dto.Value}" };
        }
    }
}
