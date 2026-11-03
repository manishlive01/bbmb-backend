using BBMB.BudgetService.Features.Works.GetList.Dtos;
using BBMB.BudgetService.Features.Works.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Works.GetByDivision
{
    public class GetWorksByDivisionHandler : IRequestHandler<GetWorksByDivisionQuery, PagedResponse<GetWorkListResponseDto>>
    {
        private readonly BudgetContext _context;
        private readonly IBudgetActorSource _actorSource;
        private readonly IBudgetOfficeScope _officeScope;
        private readonly WorkRules _rules;

        public GetWorksByDivisionHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope, WorkRules rules)
        {
            _context = context;
            _actorSource = actorSource;
            _officeScope = officeScope;
            _rules = rules;
        }

        public async Task<PagedResponse<GetWorkListResponseDto>> Handle(GetWorksByDivisionQuery request, CancellationToken ct)
        {
            // Office scope (design §9.3, BUD-031): a Division outside the actor's visible set yields no rows.
            // An unknown division code also yields no rows (it previously dereferenced null).
            var visibleDivisions = await _rules.GetVisibleDivisionIdsAsync(_actorSource, _officeScope, ct);
            var divisionId = await _context.Divisions.AsNoTracking()
                .Where(d => d.DivisionCode == request.DivisionCode)
                .Select(d => (int?)d.Id)
                .FirstOrDefaultAsync(ct);
            var query = _context.Works
                .Include(w => w.Division)
                .Where(w => divisionId != null && w.DivisionId == divisionId && visibleDivisions.Contains(w.DivisionId));

            var totalCount = await query.CountAsync(ct);

            var pageSize = Math.Clamp(request.PageSize <= 0 ? 10 : request.PageSize, 1, 100);
            var pageNumber = Math.Max(request.PageNumber, 1);
            var items = await query
                .OrderBy(w => w.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(w => new GetWorkListResponseDto
                {
                    Id = w.Id,
                    WorkCode = w.WorkCode,
                    WorkDesc = w.WorkDesc,
                    DivisionId = w.DivisionId,
                    DivisionCode = w.Division != null ? w.Division.DivisionCode : 0,
                    DivisionDesc = w.Division != null ? w.Division.DivisionDesc : null,
                    WorkClass = w.WorkClass,
                    PrecedingAmrWorkId = w.PrecedingAmrWorkId,
                    AccountCode = $"{w.MajorAccCode}-{w.SubMajorAccCode}-{w.MinorAccCode}-{w.SubMinorAccCode}-{w.DetailAccCode}-{w.ObjectAccCode}",
                    Status = w.Status,
                    Nature = w.Nature,
                    UserCode = w.UserCode
                })
                .ToListAsync(ct);

            return new PagedResponse<GetWorkListResponseDto>
            {
                Message = "Works retrieved successfully",
                Data = items,
                Pagination = new PaginationMetadata
                {
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    HasPreviousPage = pageNumber > 1,
                    HasNextPage = pageNumber < (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            };
        }
    }
}
