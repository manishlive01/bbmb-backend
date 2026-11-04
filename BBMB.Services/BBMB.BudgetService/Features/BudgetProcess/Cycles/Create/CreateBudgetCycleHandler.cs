using BBMB.BudgetService.Features.BudgetProcess.Cycles.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Cycles.Create
{
    /// <summary>
    /// Design §8.2: both years exist in fa_financial_years (FIN_YEAR_MISSING, interim under OI-13, naming the
    /// missing years; formerly M-05, whose Account Code wording misled); next = current + 1; one cycle per current
    /// year and at most one first cycle (T-14 UKs). The two uniqueness refusals have no SRS §7 number and use
    /// M-19 (catalogue gap OI-13); the UKs remain the backstop.
    /// </summary>
    public sealed class CreateBudgetCycleHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope)
        : IRequestHandler<CreateBudgetCycleCommand, Response<long>>
    {
        public const string ActionCode = "CYCLE_CREATE";

        public async Task<Response<long>> Handle(CreateBudgetCycleCommand request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail<long>(BudgetMessages.M20);

            var current = request.Dto.CurrentFinYear;
            var next = current + 1;
            var years = await context.FinancialYears.AsNoTracking()
                .Where(y => y.FinYearClosing == current || y.FinYearClosing == next).Select(y => y.FinYearClosing).ToListAsync(ct);
            var missing = new[] { current, next }.Where(y => !years.Contains(y)).ToList();
            if (missing.Count > 0)
                return FinYearMissing(missing);
            if (await context.BudgetCycles.AsNoTracking().AnyAsync(c => c.CurrentFinYear == current, ct))
                return BudgetResponses.Fail<long>(BudgetMessages.M19);
            if (request.Dto.IsFirstCycle && await context.BudgetCycles.AsNoTracking().AnyAsync(c => c.IsFirstCycle, ct))
                return BudgetResponses.Fail<long>(BudgetMessages.M19);

            var cycle = new BudgetCycle
            {
                CurrentFinYear = current, NextFinYear = next, IsFirstCycle = request.Dto.IsFirstCycle, Status = CycleStatus.Open,
            };
            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            context.BudgetCycles.Add(cycle);
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new Response<long> { IsSucced = true, Message = "Budget cycle created", Data = cycle.CycleId };
        }
        /// <summary>The FIN_YEAR_MISSING refusal naming each missing year as the FA master shows it ("2030-31" for closing 2031).</summary>
        private static Response<long> FinYearMissing(IReadOnlyList<int> closingYears)
        {
            var labels = closingYears.Select(y => $"{y - 1}-{y % 100:D2}").ToList();
            var text = labels.Count == 1
                ? $"Financial year {labels[0]} is not defined in the Financial Year master."
                : $"Financial years {string.Join(" and ", labels)} are not defined in the Financial Year master.";
            return new Response<long>
            {
                IsSucced = false, MessageId = BudgetMessages.FinYearMissing,
                Message = $"{BudgetMessages.FinYearMissing}: {text} Add {(labels.Count == 1 ? "it" : "them")} before creating this budget cycle.",
            };
        }
    }
}
