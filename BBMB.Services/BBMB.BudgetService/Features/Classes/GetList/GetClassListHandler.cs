using BBMB.BudgetService.Features.Classes.Entities;
using BBMB.BudgetService.Features.Classes.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Classes.GetList
{
    public class GetClassListHandler : IRequestHandler<GetClassListQuery, PagedResponse<GetClassListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetClassListHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<GetClassListResponseDto>> Handle(GetClassListQuery request, CancellationToken ct)
        {
            var query = _context.Classes.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.ClassCode))
                query = query.Where(c => c.ClassCode.Contains(request.ClassCode));

            if (!string.IsNullOrEmpty(request.ClassDesc))
                query = query.Where(c => c.ClassDesc.Contains(request.ClassDesc));

            if (request.ModificationDateFrom.HasValue)
                query = query.Where(c => c.ModificationDate >= request.ModificationDateFrom.Value);

            if (request.ModificationDateTo.HasValue)
                query = query.Where(c => c.ModificationDate <= request.ModificationDateTo.Value);

            // Get total count
            var totalCount = await query.CountAsync(ct);

            // Apply sorting
            query = ApplySorting(query, request.SortBy, request.SortDescending);

            // Apply pagination
            var pageSize = Math.Min(request.PageSize, 100);
            var items = await query
                .Skip((request.PageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new GetClassListResponseDto
                {
                    ClassCode = c.ClassCode,
                    ClassDesc = c.ClassDesc,
                    ModificationDate = c.ModificationDate
                })
                .ToListAsync(ct);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResponse<GetClassListResponseDto>
            {
                Message = "Classes retrieved successfully",
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

        private IQueryable<Class> ApplySorting(IQueryable<Class> query, string? sortBy, bool sortDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(c => c.ClassCode);

            return sortBy.ToLower() switch
            {
                "classcode" => sortDescending ? query.OrderByDescending(c => c.ClassCode) : query.OrderBy(c => c.ClassCode),
                "classdesc" => sortDescending ? query.OrderByDescending(c => c.ClassDesc) : query.OrderBy(c => c.ClassDesc),
                "modificationdate" => sortDescending ? query.OrderByDescending(c => c.ModificationDate) : query.OrderBy(c => c.ModificationDate),
                _ => query.OrderBy(c => c.ClassCode)
            };
        }
    }
}
