using BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.Assign
{
    /// <summary>
    /// F-17 Assign (design §8.4 assignment rows; BUD-168, BUD-171; DDD T-21, DB-01, DB-03). Order: actor (M-20) →
    /// office absent NOT_FOUND → office not an active DIVISION, CIRCLE or CE M-22 (DB-01) → every code exists and is not
    /// marked not for use, else M-05 and nothing is assigned → codes already live at the office are skipped and named in
    /// the message → the rest are written in one ASSIGN_HEADS scope with <c>source = AFA</c>. Returns the number assigned.
    /// </summary>
    public sealed class AssignHeadsHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope)
        : IRequestHandler<AssignHeadsCommand, Response<int>>
    {
        public const string ActionCode = "ASSIGN_HEADS";
        private static readonly string[] AssignableLevels = [OfficeLevel.Division, OfficeLevel.Circle, OfficeLevel.Ce];

        public async Task<Response<int>> Handle(AssignHeadsCommand request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail<int>(BudgetMessages.M20);
            var dto = request.Dto;
            var office = await context.BudgetOffices.AsNoTracking().FirstOrDefaultAsync(o => o.OfficeId == dto.OfficeId, ct);
            if (office is null)
                return BudgetResponses.Fail<int>(BudgetMessages.NotFound);
            if (!office.IsActive || !AssignableLevels.Contains(office.OfficeLevel))
                return BudgetResponses.Fail<int>(BudgetMessages.M22);
            var codes = dto.AccountCodeIds.Distinct().ToList();
            var usable = await context.HeadOfAccounts.AsNoTracking()
                .Where(h => codes.Contains(h.AccountCodeId) && !h.NotForUse).Select(h => h.AccountCodeId).ToListAsync(ct);
            if (usable.Count != codes.Count)
                return BudgetResponses.Fail<int>(BudgetMessages.M05);

            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            var live = await context.AccountHeadAssignments.AsNoTracking()
                .Where(a => a.OfficeId == office.OfficeId && a.RemovedAt == null && codes.Contains(a.AccountCodeId))
                .Select(a => a.AccountCodeId).ToListAsync(ct);
            var toAssign = codes.Except(live).ToList();
            context.AccountHeadAssignments.AddRange(toAssign.Select(c => new AccountHeadAssignment
            {
                OfficeId = office.OfficeId, AccountCodeId = c, Source = AssignmentSource.Afa,
            }));
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            var message = live.Count == 0
                ? $"{toAssign.Count} head(s) assigned"
                : $"{toAssign.Count} head(s) assigned; already assigned and skipped: {string.Join(", ", live.OrderBy(c => c))}";
            return new Response<int> { IsSucced = true, Message = message, Data = toAssign.Count };
        }
    }
}
