using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Features.Works.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.Works.Create
{
    public class CreateWorkHandler : IRequestHandler<CreateWorkCommand, Response>
    {
        private readonly BudgetContext _context;
        private readonly IBudgetActorSource _actorSource;
        private readonly IBudgetActionScope _scope;
        private readonly WorkRules _rules;
        public CreateWorkHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, WorkRules rules)
        {
            _context = context;
            _actorSource = actorSource;
            _scope = scope;
            _rules = rules;
        }
        public async Task<Response> Handle(CreateWorkCommand request, CancellationToken ct)
        {
            // work carries fn_require_actor from P1 (DB-28): every write runs inside the action scope.
            var actor = await _actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail(BudgetMessages.M20);

            // ================= DESIGN §11.3 ORDER =================
            // (1) Division: a DDO keys works only for its own Division (BUD-052), else 403 M-20, nothing written.
            var actorDivisionId = await _context.BudgetOffices.AsNoTracking()
                .Where(o => o.OfficeId == actor.OfficeId && o.OfficeLevel == OfficeLevel.Division)
                .Select(o => o.DivisionId)
                .FirstOrDefaultAsync(ct);
            if (actorDivisionId is null || actorDivisionId != request.dto.DivisionId)
                return BudgetResponses.Fail(BudgetMessages.M20);
            var divisionId = actorDivisionId.Value;

            // (2) First-cycle guard (design §5.3): LEGACY_OPENING means a continuing work keyed in the first cycle.
            if (await _rules.CheckFirstCycleAsync(actor.OfficeId, ct) is { } guard)
                return BudgetResponses.Fail(guard);

            // (3) accountCodeId → six segments (design §5.2); absent or not for use → M-05.
            var (code, codeFailure) = await _rules.ResolveAccountCodeAsync(request.dto.AccountCodeId, ct);
            if (code is null)
                return BudgetResponses.Fail(codeFailure!);

            if (!string.IsNullOrEmpty(request.dto.DesignationCode))
            {
                var designationExists = await _context.Designations.AsNoTracking()
                    .AnyAsync(d => d.DesignationCode == request.dto.DesignationCode, ct);
                if (!designationExists)
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Designation code '{request.dto.DesignationCode}' not found"
                    };
            }

            // ================= AUTO GENERATE WORK CODE (removed in P4, design §4.6) =================
            var lastWorkCode = await _context.Works
                .Where(w => w.DivisionId == divisionId)
                .OrderByDescending(w => w.WorkCode)
                .Select(w => w.WorkCode)
                .FirstOrDefaultAsync(ct);
            int nextNumber = 1;
            var lastWorkCodeStr = lastWorkCode.ToString();
            var divisionStr = divisionId.ToString();
            if (!string.IsNullOrEmpty(lastWorkCodeStr) &&
                lastWorkCodeStr.StartsWith(divisionStr) &&
                lastWorkCodeStr.Length > divisionStr.Length)
            {
                var numericPart = lastWorkCodeStr.Substring(divisionStr.Length);
                if (int.TryParse(numericPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }
            var newWorkCode = $"{divisionId}{nextNumber.ToString("D5")}";
            request.dto.WorkCode = newWorkCode;

            // ================= CREATE WORK =================
            var work = request.dto.Adapt<Work>();
            WorkRules.ApplySegments(work, code);
            // Set server-side after Adapt, never from the request body (design §9.3, §11.3): a work keyed on
            // the work master screen is a continuing work keyed in the first cycle (DDD T-23, L-08).
            work.EnteredVia = WorkEnteredVia.LegacyOpening;
            work.PreDdd = false;
            await using var tx = await _scope.BeginAsync("WORKS_ENTER", ct: ct);
            await _context.Works.AddAsync(work, ct);
            await _context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new Response
            {
                IsSucced = true,
                Message = $"Work created successfully with WorkCode {newWorkCode}"
            };
        }
    }
}
