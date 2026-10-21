using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.SubMinor.Entities;
using BBMB.BudgetService.Features.DetailHeads.Entities;
using BBMB.BudgetService.Features.HeadOfAccounts.Create.Dtos;
using BBMB.BudgetService.Features.HeadOfAccounts.Entities;
using BBMB.BudgetService.Features.HeadOfAccounts.Rules;
using BBMB.BudgetService.Features.ObjectClassifications.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.HeadOfAccounts.Create
{
    /// <summary>
    /// F-01 Create (design §8.4; BUD-039 to BUD-046, BUD-267, BUD-322). Order: actor (M-20) → per level, a selected
    /// code with a description M-01, a new code without one M-21 (every failure listed) → the complete code exists
    /// M-02 → in one F01_CREATE scope the new segments and the T-07 row (BUD-045), <c>origin = CREATED</c>, the
    /// e-Office reference, and <c>party_emp_required</c> by BUD-267 (never from the request). The code is available
    /// for selection on commit (BUD-046). The six segment PKs and the complete-code UK are the database backstops
    /// (23505 → M-01 / M-02).
    /// </summary>
    public sealed class CreateHeadOfAccountHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, AccountCodeRules rules)
        : IRequestHandler<CreateHeadOfAccountCommand, Response>
    {
        public const string ActionCode = "F01_CREATE";
        /// <summary>Legacy NOT NULL <c>user_code</c> on T-01 to T-07 rows written by F-01; the officer is in the [A]/[C] columns.</summary>
        public const string F01UserCode = "BUDF01";

        public async Task<Response> Handle(CreateHeadOfAccountCommand request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail(BudgetMessages.M20);
            var d = request.Dto;
            var state = await rules.ReadSegmentsAsync(d, ct);
            var failures = AccountCodeRules.CheckSegments(d, state);
            if (failures.Count > 0)
                return BudgetResponses.RuleFailures(new RuleFailuresDto { Failures = failures });
            if (await rules.CompleteCodeExistsAsync(d, ct))
                return BudgetResponses.Fail(BudgetMessages.M02);

            var now = DateTime.UtcNow;
            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            if (!state.Major)
                context.MajorHeads.Add(new MajorHead { MajorAccCode = d.MajorAccCode, MajorAccDesc = d.MajorAccDesc!.Trim(), ModificationDate = now, UserCode = F01UserCode });
            if (!state.SubMajor)
                context.SubMajorHeads.Add(new SubMajorHead { MajorAccCode = d.MajorAccCode, SubMajorAccCode = d.SubMajorAccCode, SubMajorAccDesc = d.SubMajorAccDesc!.Trim(), ModificationDate = now, UserCode = F01UserCode });
            if (!state.Minor)
                context.MinorHeads.Add(new MinorHead { MinorAccCode = d.MinorAccCode, MinorAccDesc = d.MinorAccDesc!.Trim(), ModificationDate = now, UserCode = F01UserCode });
            if (!state.SubMinor)
                context.SubMinorHeads.Add(new SubMinorHead { MinorAccCode = d.MinorAccCode, SubMinorAccCode = d.SubMinorAccCode, SubMinorAccDesc = d.SubMinorAccDesc!.Trim(), ModificationDate = now, UserCode = F01UserCode });
            if (!state.Detail)
                context.DetailHeads.Add(new DetailHead { DetailAccCode = d.DetailAccCode, DetailAccDesc = d.DetailAccDesc!.Trim(), ModificationDate = now, UserCode = F01UserCode });
            if (!state.Object)
                context.ObjectClassifications.Add(new ObjectClassification { ObjectAccCode = d.ObjectAccCode, ObjectAccDesc = d.ObjectAccDesc!.Trim(), ModificationDate = now, UserCode = F01UserCode });
            await context.SaveChangesAsync(ct); // the segments exist before the account code's FKs are checked

            var code = new HeadOfAccount
            {
                MajorAccCode = d.MajorAccCode, SubMajorAccCode = d.SubMajorAccCode, MinorAccCode = d.MinorAccCode,
                SubMinorAccCode = d.SubMinorAccCode, DetailAccCode = d.DetailAccCode, ObjectAccCode = d.ObjectAccCode,
                HeadDesc = d.HeadDesc.Trim(), Origin = HeadOfAccountOrigin.Created, EofficeApprovalRef = d.EofficeApprovalRef.Trim(),
                PartyEmpRequired = HeadOfAccountOrigin.PartyEmpRequiredFor(d.MajorAccCode), ModificationDate = now, UserCode = F01UserCode,
            };
            context.HeadOfAccounts.Add(code);
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            var saved = await context.HeadOfAccounts.AsNoTracking()
                .Where(h => h.MajorAccCode == d.MajorAccCode && h.SubMajorAccCode == d.SubMajorAccCode && h.MinorAccCode == d.MinorAccCode &&
                            h.SubMinorAccCode == d.SubMinorAccCode && h.DetailAccCode == d.DetailAccCode && h.ObjectAccCode == d.ObjectAccCode)
                .Select(h => new CreateAccountCodeResponseDto(h.AccountCodeId, h.CompleteCode)).SingleAsync(ct);
            return new Response<CreateAccountCodeResponseDto> { IsSucced = true, Message = "Account code created", Data = saved };
        }
    }
}
