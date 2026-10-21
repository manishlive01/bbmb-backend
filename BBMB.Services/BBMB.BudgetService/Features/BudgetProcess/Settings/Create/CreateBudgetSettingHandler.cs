using BBMB.BudgetService.Features.BudgetProcess.Settings.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Settings.Create
{
    /// <summary>
    /// Appends one T-17 row (no PUT or DELETE exists; fn_forbid_change refuses UPDATE). The unforeseen head must
    /// exist and be in use (M-05, as design §5.2). A row with the same key and instant is refused with M-19 (the
    /// cycle-uniqueness choice under OI-13); the T-17 UK maps to M-19 as the backstop under concurrency.
    /// </summary>
    public sealed class CreateBudgetSettingHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope)
        : IRequestHandler<CreateBudgetSettingCommand, Response<long>>
    {
        public const string ActionCode = "SETTING_CREATE";

        public async Task<Response<long>> Handle(CreateBudgetSettingCommand request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail<long>(BudgetMessages.M20);
            var dto = request.Dto;
            if (dto.AccountCodeId is { } code &&
                !await context.HeadOfAccounts.AsNoTracking().AnyAsync(h => h.AccountCodeId == code && !h.NotForUse, ct))
                return BudgetResponses.Fail<long>(BudgetMessages.M05);

            var effectiveFrom = dto.EffectiveFrom.ToUniversalTime();
            if (await context.BudgetSettings.AsNoTracking()
                    .AnyAsync(s => s.SettingKey == dto.SettingKey && s.EffectiveFrom == effectiveFrom, ct))
                return BudgetResponses.Fail<long>(BudgetMessages.M19);
            var row = new BudgetSetting
            {
                SettingKey = dto.SettingKey, NumericValue = dto.NumericValue, AccountCodeId = dto.AccountCodeId,
                EffectiveFrom = effectiveFrom, AuthorityRef = dto.AuthorityRef.Trim(),
            };
            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            context.BudgetSettings.Add(row);
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new Response<long> { IsSucced = true, Message = "Setting recorded", Data = row.SettingId };
        }
    }
}
