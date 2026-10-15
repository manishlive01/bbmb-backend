using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Offices.LoadFromHierarchy.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Offices.LoadFromHierarchy
{
    /// <summary>
    /// DDD §9 L-03, design §8.2 (with the implementation-time change of 2026-10-14). Idempotent:
    /// <list type="bullet">
    /// <item>FINANCE, then SECRETARIAT (parent FINANCE), then IT_CELL are created only if absent.</item>
    /// <item>An active CE is created (parent SECRETARIAT) only when its wing is supplied; otherwise it is listed
    /// in <c>cesWithoutWing</c>, because ck_office_wing requires a wing on every CE row.</item>
    /// <item>An active Circle is created under its CE's office, a Division under its Circle's office; one whose
    /// parent office is absent is listed in <c>officesWithoutParent</c>.</item>
    /// <item>An existing row is never updated (its wing included); it is counted in <c>existing</c>.</item>
    /// </list>
    /// One transaction, action LOAD_OFFICES; the T-51 rows are written by fn_audit_row.
    /// </summary>
    public sealed class LoadBudgetOfficesHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope)
        : IRequestHandler<LoadBudgetOfficesCommand, Response<LoadBudgetOfficesResponseDto>>
    {
        public const string ActionCode = "LOAD_OFFICES";

        public async Task<Response<LoadBudgetOfficesResponseDto>> Handle(LoadBudgetOfficesCommand request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail<LoadBudgetOfficesResponseDto>(BudgetMessages.M20);

            var wings = (request.Dto?.CeWings ?? []).ToDictionary(w => w.CeId, w => w.WingCode);
            var result = new LoadBudgetOfficesResponseDto();

            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            // Serialises concurrent loads: FINANCE and SECRETARIAT have no unique key, so two runs could
            // otherwise each create one. Reads stay possible (SHARE ROW EXCLUSIVE).
            await context.Database.ExecuteSqlRawAsync("LOCK TABLE budget.office IN SHARE ROW EXCLUSIVE MODE", ct);

            var offices = await context.BudgetOffices.ToListAsync(ct);

            async Task<BudgetOffice> Singleton(string level, string name, long? parentId)
            {
                var found = offices.FirstOrDefault(o => o.OfficeLevel == level);
                if (found is not null) { result.Existing++; return found; }
                var o = new BudgetOffice { OfficeLevel = level, OfficeName = name, ParentOfficeId = parentId };
                context.BudgetOffices.Add(o);
                await context.SaveChangesAsync(ct);
                offices.Add(o);
                result.Created++;
                return o;
            }
            var finance = await Singleton(OfficeLevel.Finance, "Finance", null);
            var secretariat = await Singleton(OfficeLevel.Secretariat, "Secretariat", finance.OfficeId);
            await Singleton(OfficeLevel.ItCell, "IT Cell", null);

            // CE
            var ceOffice = offices.Where(o => o.CeId != null).ToDictionary(o => o.CeId!.Value, o => o.OfficeId);
            var ces = await context.CEManagements.AsNoTracking().Where(c => c.Status)
                .OrderBy(c => c.Id).Select(c => new { c.Id, c.Name }).ToListAsync(ct);
            var newCes = new List<BudgetOffice>();
            foreach (var ce in ces)
            {
                if (ceOffice.ContainsKey(ce.Id)) { result.Existing++; continue; }
                if (!wings.TryGetValue(ce.Id, out var wing))
                {
                    result.CesWithoutWing.Add(new UnloadedOfficeDto(OfficeLevel.Ce, ce.Id, ce.Name));
                    continue;
                }
                var o = new BudgetOffice { OfficeLevel = OfficeLevel.Ce, CeId = ce.Id, WingCode = wing, ParentOfficeId = secretariat.OfficeId };
                context.BudgetOffices.Add(o);
                newCes.Add(o);
            }
            await context.SaveChangesAsync(ct);
            foreach (var o in newCes) ceOffice[o.CeId!.Value] = o.OfficeId;
            result.Created += newCes.Count;

            // Circle (parent: its CE's office, cr_management.ce_id)
            var circleOffice = offices.Where(o => o.CircleId != null).ToDictionary(o => o.CircleId!.Value, o => o.OfficeId);
            var circles = await context.CRManagements.AsNoTracking().Where(c => c.Status)
                .OrderBy(c => c.Id).Select(c => new { c.Id, c.CEId, c.Name }).ToListAsync(ct);
            var newCircles = new List<BudgetOffice>();
            foreach (var cr in circles)
            {
                if (circleOffice.ContainsKey(cr.Id)) { result.Existing++; continue; }
                if (!ceOffice.TryGetValue(cr.CEId, out var parent))
                {
                    result.OfficesWithoutParent.Add(new UnloadedOfficeDto(OfficeLevel.Circle, cr.Id, cr.Name));
                    continue;
                }
                var o = new BudgetOffice { OfficeLevel = OfficeLevel.Circle, CircleId = cr.Id, ParentOfficeId = parent };
                context.BudgetOffices.Add(o);
                newCircles.Add(o);
            }
            await context.SaveChangesAsync(ct);
            foreach (var o in newCircles) circleOffice[o.CircleId!.Value] = o.OfficeId;
            result.Created += newCircles.Count;

            // Division (parent: its Circle's office, division.cr_id). The division master has no status column,
            // so every Division row is treated as in use.
            var divisionOffices = offices.Where(o => o.DivisionId != null).Select(o => o.DivisionId!.Value).ToHashSet();
            var divisions = await context.Divisions.AsNoTracking()
                .OrderBy(d => d.Id).Select(d => new { d.Id, d.CRId, d.DivisionDesc }).ToListAsync(ct);
            var newDivisions = 0;
            foreach (var d in divisions)
            {
                if (divisionOffices.Contains(d.Id)) { result.Existing++; continue; }
                if (!circleOffice.TryGetValue(d.CRId, out var parent))
                {
                    result.OfficesWithoutParent.Add(new UnloadedOfficeDto(OfficeLevel.Division, d.Id, d.DivisionDesc));
                    continue;
                }
                context.BudgetOffices.Add(new BudgetOffice { OfficeLevel = OfficeLevel.Division, DivisionId = d.Id, ParentOfficeId = parent });
                newDivisions++;
            }
            await context.SaveChangesAsync(ct);
            result.Created += newDivisions;

            await tx.CommitAsync(ct);
            return new Response<LoadBudgetOfficesResponseDto>
            {
                IsSucced = true,
                Message = result.CesWithoutWing.Count == 0 && result.OfficesWithoutParent.Count == 0
                    ? "Offices loaded"
                    : "Offices loaded in part: CEs without a wing and their Circles and Divisions are listed",
                Data = result,
            };
        }
    }
}
