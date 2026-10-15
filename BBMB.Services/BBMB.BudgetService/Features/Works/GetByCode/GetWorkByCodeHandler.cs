using BBMB.BudgetService.Features.Works.GetById.Dtos;
using BBMB.BudgetService.Features.Works.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Works.GetByCode
{
    // P4 (design §4.6): addresses a work by its string work code. Scoped the same way as GET /api/works/{id}
    // (design §4.9, BUD-031): a work outside the actor's visible Divisions reads as not found. work_code is
    // nullable after the reset, so a null/blank code matches nothing.
    public sealed class GetWorkByCodeHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope, WorkRules rules)
        : IRequestHandler<GetWorkByCodeQuery, GetWorkByIdResponseDto>
    {
        public async Task<GetWorkByIdResponseDto> Handle(GetWorkByCodeQuery request, CancellationToken ct)
        {
            var visibleDivisions = await rules.GetVisibleDivisionIdsAsync(actorSource, officeScope, ct);
            var work = await context.Works
                .Include(w => w.Division)
                .FirstOrDefaultAsync(w => w.WorkCode == request.WorkCode && visibleDivisions.Contains(w.DivisionId), ct);

            if (work == null)
                throw new KeyNotFoundException($"Work with code '{request.WorkCode}' not found");

            var response = work.Adapt<GetWorkByIdResponseDto>();
            response.DivisionDesc = work.Division?.DivisionDesc;
            response.AccountCode = $"{work.MajorAccCode}-{work.SubMajorAccCode}-{work.MinorAccCode}-{work.SubMinorAccCode}-{work.DetailAccCode}-{work.ObjectAccCode}";

            response.AccountDescription = await context.HeadOfAccounts
                .Where(h => h.MajorAccCode == work.MajorAccCode && h.SubMajorAccCode == work.SubMajorAccCode &&
                            h.MinorAccCode == work.MinorAccCode && h.SubMinorAccCode == work.SubMinorAccCode &&
                            h.DetailAccCode == work.DetailAccCode && h.ObjectAccCode == work.ObjectAccCode)
                .Select(h => h.HeadDesc)
                .FirstOrDefaultAsync(ct);

            return response;
        }
    }
}
