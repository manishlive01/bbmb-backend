using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Features.Works.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.Works.Update
{
    /// <summary>
    /// Design §11.3 row 2. Field rules, each checked against the stored row: <c>division_id</c> never changes;
    /// <c>work_desc</c> in any cycle; <c>accountCodeId</c>, <c>workClass</c> and the AM&amp;R predecessor only while
    /// <see cref="WorkRules.IsReferencedAsync"/> is false (else M-19); clearing <c>pre_ddd</c> needs
    /// <c>workClass</c> and the first-cycle guard, and sets <c>entered_via = LEGACY_OPENING</c> server-side.
    /// </summary>
    public class UpdateWorkHandler : IRequestHandler<UpdateWorkCommand, Response>
    {
        private readonly BudgetContext _context;
        private readonly IBudgetActorSource _actorSource;
        private readonly IBudgetActionScope _scope;
        private readonly WorkRules _rules;
        public UpdateWorkHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, WorkRules rules)
        {
            _context = context;
            _actorSource = actorSource;
            _scope = scope;
            _rules = rules;
        }
        public async Task<Response> Handle(UpdateWorkCommand request, CancellationToken ct)
        {
            // work carries fn_require_actor from P1 (DB-28): every write runs inside the action scope.
            var actor = await _actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail(BudgetMessages.M20);

            // Division: only a work of the actor's Division is addressed; any other is 404 (design §4.9).
            var actorDivisionId = await _context.BudgetOffices.AsNoTracking()
                .Where(o => o.OfficeId == actor.OfficeId && o.OfficeLevel == OfficeLevel.Division)
                .Select(o => o.DivisionId)
                .FirstOrDefaultAsync(ct);
            // P4 (design §4.6): a work is addressed by its stable id, not by work_code (now string?/null until
            // generated). The route becomes PUT /api/works/{id:int} (P4-03 renames the pattern).
            var work = actorDivisionId is null
                ? null
                : await _context.Works.FirstOrDefaultAsync(w => w.Id == request.Id && w.DivisionId == actorDivisionId, ct);
            if (work == null)
                return BudgetResponses.Fail(BudgetMessages.NotFound);

            var dto = request.dto;
            var classChanges =
                (dto.WorkClass is not null && dto.WorkClass != work.WorkClass) ||
                (dto.PrecedingAmrWorkId is not null && dto.PrecedingAmrWorkId != work.PrecedingAmrWorkId);
            var headChangeRequested = dto.AccountCodeId is not null;

            // Reference rule first, before the code lookup (design §5.2): once a record references the work,
            // its head and class are part of that record; the change goes through F-04 revision (BUD-269).
            if ((classChanges || headChangeRequested) && await _rules.IsReferencedAsync(work.Id, ct))
            {
                var headDiffers = false;
                if (headChangeRequested)
                {
                    var current = await _context.HeadOfAccounts.AsNoTracking()
                        .AnyAsync(h => h.AccountCodeId == dto.AccountCodeId &&
                                       h.MajorAccCode == work.MajorAccCode && (h.SubMajorAccCode ?? "") == work.SubMajorAccCode &&
                                       h.MinorAccCode == work.MinorAccCode && (h.SubMinorAccCode ?? "") == work.SubMinorAccCode &&
                                       h.DetailAccCode == work.DetailAccCode && h.ObjectAccCode == work.ObjectAccCode, ct);
                    headDiffers = !current;
                }
                if (classChanges || headDiffers)
                    return BudgetResponses.Fail(BudgetMessages.M19);
            }

            if (headChangeRequested)
            {
                var (code, codeFailure) = await _rules.ResolveAccountCodeAsync(dto.AccountCodeId!.Value, ct);
                if (code is null)
                    return BudgetResponses.Fail(codeFailure!);
                if (!WorkRules.HasSegments(work, code))
                    WorkRules.ApplySegments(work, code);
            }

            // Clearing pre_ddd (design §5.2 row a, §11.3): workClass supplied, first-cycle guard, server-side origin.
            if (work.PreDdd && dto.WorkClass is not null)
            {
                if (await _rules.CheckFirstCycleAsync(actor.OfficeId, ct) is { } guard)
                    return BudgetResponses.Fail(guard);
                work.PreDdd = false;
                work.EnteredVia = WorkEnteredVia.LegacyOpening;
            }
            if (dto.WorkClass is not null)
                work.WorkClass = dto.WorkClass;
            if (dto.PrecedingAmrWorkId is not null)
                work.PrecedingAmrWorkId = dto.PrecedingAmrWorkId;

            if (!string.IsNullOrEmpty(dto.DesignationCode))
            {
                var designationExists = await _context.Designations
                    .AnyAsync(d => d.DesignationCode == dto.DesignationCode, ct);
                if (!designationExists)
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Designation code '{dto.DesignationCode}' not found"
                    };
            }

            // ================= UPDATE THE REMAINING FIELDS =================
            work.WorkDesc = dto.WorkDesc;
#pragma warning disable CS0618 // Deprecated, not dropped (D-5, design §5.2); kept writable as before.
            work.OriginalBudget = dto.OriginalBudget ?? string.Empty;
            work.RevisedBudget = dto.RevisedBudget ?? string.Empty;
            work.ProposedBudget = dto.ProposedBudget ?? string.Empty;
#pragma warning restore CS0618
            work.Remarks = dto.Remarks ?? string.Empty;
            work.UserCode = dto.UserCode;
            work.FinYrStart = dto.FinYrStart;
            work.FinYrEnd = dto.FinYrEnd;
            work.SanctionNo = dto.SanctionNo;
            work.SanctionDate = dto.SanctionDate;
            work.DesignationCode = dto.DesignationCode;
            work.EstimatedCost = dto.EstimatedCost;
            work.Status = dto.Status;
            work.Nature = dto.Nature;
            work.RefWorkCode = dto.RefWorkCode;
            await using var tx = await _scope.BeginAsync("WORKS_UPDATE", ct: ct);
            await _context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new Response
            {
                IsSucced = true,
                Message = "Work updated successfully"
            };
        }
    }
}
