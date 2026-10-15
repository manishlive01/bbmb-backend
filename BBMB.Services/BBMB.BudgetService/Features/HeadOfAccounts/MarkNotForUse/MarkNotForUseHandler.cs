using BBMB.BudgetService.Features.HeadOfAccounts.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.HeadOfAccounts.MarkNotForUse
{
    /// <summary>
    /// BUD-308 (design §8.4): a code created in error is marked not for use, never altered or deleted, and a correct code
    /// is created afresh. Order: actor (M-20) → code absent NOT_FOUND → already marked M-19 (the mark is one-way) →
    /// <see cref="AccountCodeRules.CheckMarkableAsync"/> (M-33) → in one F01_MARK_NFU scope the mark with its three
    /// particulars, the officer from the session. DB-03 (trg_head_master_protect) is the backstop.
    /// </summary>
    public sealed class MarkNotForUseHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, AccountCodeRules rules,
        TimeProvider? clock = null)
        : IRequestHandler<MarkNotForUseCommand, Response>
    {
        public const string ActionCode = "F01_MARK_NFU";

        public async Task<Response> Handle(MarkNotForUseCommand request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail(BudgetMessages.M20);
            var code = await context.HeadOfAccounts.FirstOrDefaultAsync(h => h.AccountCodeId == request.AccountCodeId, ct);
            if (code is null)
                return BudgetResponses.Fail(BudgetMessages.NotFound);
            if (code.NotForUse)
                return BudgetResponses.Fail(BudgetMessages.M19);
            if (await rules.CheckMarkableAsync(code.AccountCodeId, ct) is { } refusal)
                return BudgetResponses.Fail(refusal);
            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            code.NotForUse = true;
            code.NotForUseRef = request.Dto.EofficeRef.Trim();
            code.NotForUseAt = (clock ?? TimeProvider.System).GetUtcNow();
            code.NotForUseBy = actor.EmployeeId;
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new Response { IsSucced = true, Message = $"Account code {code.CompleteCode} marked not for use" };
        }
    }
}
