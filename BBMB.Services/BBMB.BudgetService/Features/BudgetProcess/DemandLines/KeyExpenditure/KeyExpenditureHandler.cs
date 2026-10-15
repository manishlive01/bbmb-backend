using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Entities;
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
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.KeyExpenditure
{
    /// <summary>
    /// Design §8.3 KeyExpenditure, §4.11. The expenditure source is KEYED: T-18 is absent before P3 and the
    /// Financial Accounting gateway is not available, so the origin office keys the figure (BUD-312). Checks as
    /// every origin write (<see cref="DemandLineWrites"/>: S-01 / S-04, the states with figures open to the origin).
    /// The subject is the line's: its work (WORK) or the origin office and head (HEAD); an UNFORESEEN line has
    /// none (M-19). The nature follows the category: RECEIPT for RECEIPTS, EXPENDITURE otherwise (M-22).
    /// FULL_YEAR is the previous year (Table 3 col 6), UP_TO_MONTH / UP_TO_DATE the current year (col 10).
    /// One T-35 row is appended; earlier rows are kept (append-only, BUD-025) and the latest is current. The
    /// officer is recorded in the [C] columns by <c>fn_require_actor</c> (BUD-024).
    /// </summary>
    public sealed class KeyExpenditureHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, IBudgetOfficeScope officeScope,
        IAuthorizationService authorization, IHttpContextAccessor httpContextAccessor, IConfiguration configuration,
        TimeProvider? clock = null)
        : IRequestHandler<KeyExpenditureCommand, Response>
    {
        public const string ActionCode = "EXPENDITURE_KEY";

        public async Task<Response> Handle(KeyExpenditureCommand request, CancellationToken ct)
        {
            var now = (clock ?? TimeProvider.System).GetUtcNow();
            var target = await DemandLineWrites.ResolveAsync(
                context, actorSource, officeScope, authorization, httpContextAccessor, request.ProposalId, now, ct);
            if (target.Refusal is { } refusal)
                return refusal;
            var (actor, p) = (target.Actor!, target.Proposal!);
            var (line, lineRefusal) = await DemandLineWrites.FindLiveLineAsync(context, p, request.DemandLineId, false, ct);
            if (lineRefusal is not null)
                return lineRefusal;
            if (line!.LineType == DemandLineType.Unforeseen)
                return BudgetResponses.Fail(BudgetMessages.M19);
            var dto = request.Dto;
            var expectedNature = p.Category == ProposalCategory.Receipts ? ExpenditureValues.Receipt : ExpenditureValues.Expenditure;
            if (dto.Nature != expectedNature)
                return DemandLineWrites.FieldFailure(line, "nature", BudgetMessages.M22);
            var currentYear = await context.BudgetCycles.AsNoTracking()
                .Where(c => c.CycleId == p.CycleId).Select(c => c.CurrentFinYear).SingleAsync(ct);
            var isWork = line.LineType == DemandLineType.Work;
            var row = new ExpenditureFigure
            {
                SubjectType = isWork ? "WORK" : "HEAD",
                WorkId = isWork ? line.WorkId : null,
                OfficeId = isWork ? null : p.OriginOfficeId,
                AccountCodeId = isWork ? null : line.AccountCodeId,
                Nature = dto.Nature, Period = dto.Period, UpToMonth = dto.UpToMonth, Amount = dto.Amount,
                FinYear = dto.Period == ExpenditureValues.FullYear ? currentYear - 1 : currentYear,
                Source = ExpenditureValues.Keyed, AsAt = now,
            };
            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            context.ExpenditureFigures.Add(row);
            DemandLineWrites.RenewLease(p, actor, now, ProposalLease.Length(configuration));
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new Response<long> { IsSucced = true, Message = "Expenditure recorded", Data = row.ExpenditureFigureId };
        }
    }
}
