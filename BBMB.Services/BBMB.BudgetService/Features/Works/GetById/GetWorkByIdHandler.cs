using BBMB.BudgetService.Features.Works.GetById.Dtos;
using BBMB.BudgetService.Features.Works.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Works.GetById
{
    public class GetWorkByIdHandler : IRequestHandler<GetWorkByIdQuery, GetWorkByIdResponseDto>
    {
        private readonly BudgetContext _context;
        private readonly IBudgetActorSource _actorSource;
        private readonly IBudgetOfficeScope _officeScope;
        private readonly WorkRules _rules;

        public GetWorkByIdHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope, WorkRules rules)
        {
            _context = context;
            _actorSource = actorSource;
            _officeScope = officeScope;
            _rules = rules;
        }

        public async Task<GetWorkByIdResponseDto> Handle(GetWorkByIdQuery request, CancellationToken ct)
        {
            // A work outside the actor's visible Divisions is reported as not found (design §4.9, BUD-031).
            var visibleDivisions = await _rules.GetVisibleDivisionIdsAsync(_actorSource, _officeScope, ct);
            // P4 (design §4.6): a work is addressed by its stable id, not by work_code (which is now string?
            // and null until generated). The route becomes GET /api/works/{id:int} (P4-03 renames the pattern
            // and adds /api/works/by-code/{workCode}); the handler filters by id here.
            var work = await _context.Works
                .Include(w => w.Division)
                .FirstOrDefaultAsync(w => w.Id == request.Id && visibleDivisions.Contains(w.DivisionId), ct);

            if (work == null)
                throw new KeyNotFoundException($"Work with id '{request.Id}' not found");

            var response = work.Adapt<GetWorkByIdResponseDto>();
            response.DivisionDesc = work.Division?.DivisionDesc;
            response.AccountCode = $"{work.MajorAccCode}-{work.SubMajorAccCode}-{work.MinorAccCode}-{work.SubMinorAccCode}-{work.DetailAccCode}-{work.ObjectAccCode}";

            // Get account description from HeadOfAccount if exists
            var accountDescription = await GetAccountDescription(work.MajorAccCode, work.SubMajorAccCode,
                work.MinorAccCode, work.SubMinorAccCode, work.DetailAccCode, work.ObjectAccCode, ct);
            response.AccountDescription = accountDescription;

            return response;
        }

        private async Task<string?> GetAccountDescription(string majorCode, string subMajorCode,
            string minorCode, string subMinorCode, string detailCode, string objectCode, CancellationToken ct)
        {
            // Query HeadOfAccount table to get full description
            var headOfAccount = await _context.HeadOfAccounts
                .FirstOrDefaultAsync(h =>
                    h.MajorAccCode == majorCode &&
                    h.SubMajorAccCode == subMajorCode &&
                    h.MinorAccCode == minorCode &&
                    h.SubMinorAccCode == subMinorCode &&
                    h.DetailAccCode == detailCode &&
                    h.ObjectAccCode == objectCode, ct);

            return headOfAccount?.HeadDesc;
        }
    }
}
