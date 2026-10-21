using BBMB.BudgetService.Features.Estimates.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.Estimates.GetByWork
{
    // P4 (design §4.6): filter by work_id (the post-reset key), not the retained int work_code.
    public class GetEstimatesByWorkHandler : IRequestHandler<GetEstimatesByWorkQuery, PagedResponse<GetEstimateListResponseDto>>
    {
        private readonly BudgetContext _context;
        public GetEstimatesByWorkHandler(BudgetContext context)
        {
            _context = context;
        }

#pragma warning disable CS0618 // work_code retained read-only through P4 (design §4.6 step 2, OI-46)
        public async Task<PagedResponse<GetEstimateListResponseDto>> Handle(GetEstimatesByWorkQuery request, CancellationToken ct)
        {
            var query = _context.Estimates.Include(e => e.Work).Where(e => e.WorkId == request.WorkId);
            var totalCount = await query.CountAsync(ct);

            var pageSize = Math.Min(request.PageSize, 100);
            var estimates = await query
                .OrderBy(e => e.EstimateCode)
                .Skip((request.PageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            var divisionCodes = estimates.Select(e => e.DivisionCode).Distinct().ToList();
            var divisions = await _context.Divisions
                .Where(d => divisionCodes.Contains(d.DivisionCode))
                .ToDictionaryAsync(d => d.DivisionCode, d => d.DivisionDesc, ct);

            var items = estimates.Select(e => new GetEstimateListResponseDto
            {
                WorkCode = e.WorkCode,
                EstimateCode = e.EstimateCode,
                EstimateDesc = e.EstimateDesc,
                EstimateAmt = e.EstimateAmt,
                FinYearEnd = e.FinYearEnd,
                DivisionCode = e.DivisionCode,
                WorkDescription = e.Work?.WorkDesc,
                DivisionDescription = divisions.GetValueOrDefault(e.DivisionCode)
            }).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            return new PagedResponse<GetEstimateListResponseDto>
            {
                Message = "Estimates retrieved successfully",
                Data = items,
                Pagination = new PaginationMetadata
                {
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    HasPreviousPage = request.PageNumber > 1,
                    HasNextPage = request.PageNumber < totalPages
                }
            };
        }
#pragma warning restore CS0618
    }
}
