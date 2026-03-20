using BBMB.BudgetService.Features.Estimates.Entities;
using BBMB.BudgetService.Features.Estimates.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Estimates.GetList
{
    public class GetEstimateListHandler : IRequestHandler<GetEstimateListQuery, PagedResponse<GetEstimateListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetEstimateListHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<GetEstimateListResponseDto>> Handle(GetEstimateListQuery request, CancellationToken ct)
        {
            var query = _context.Estimates
                .Include(e => e.Work)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.WorkCode.ToString()))
                query = query.Where(e => e.WorkCode == request.WorkCode);

            if (request.DivisionCode.HasValue)
                query = query.Where(e => e.DivisionCode == request.DivisionCode.Value);

            if (request.FinYearEnd.HasValue)
                query = query.Where(e => e.FinYearEnd == request.FinYearEnd.Value);

            // Get total count
            var totalCount = await query.CountAsync(ct);

            // Apply sorting
            query = ApplySorting(query, request.SortBy, request.SortDescending);

            // Apply pagination
            var pageSize = Math.Min(request.PageSize, 100);
            var estimates = await query
                .Skip((request.PageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            // Get division descriptions
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

        private IQueryable<Estimate> ApplySorting(IQueryable<Estimate> query, string? sortBy, bool sortDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(e => e.WorkCode).ThenBy(e => e.EstimateCode);

            return sortBy.ToLower() switch
            {
                "workcode" => sortDescending ? query.OrderByDescending(e => e.WorkCode) : query.OrderBy(e => e.WorkCode),
                "estimatecode" => sortDescending ? query.OrderByDescending(e => e.EstimateCode) : query.OrderBy(e => e.EstimateCode),
                "estimateamt" => sortDescending ? query.OrderByDescending(e => e.EstimateAmt) : query.OrderBy(e => e.EstimateAmt),
                "finyearend" => sortDescending ? query.OrderByDescending(e => e.FinYearEnd) : query.OrderBy(e => e.FinYearEnd),
                _ => query.OrderBy(e => e.WorkCode).ThenBy(e => e.EstimateCode)
            };
        }
    }
}
