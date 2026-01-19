using BBMB.BudgetService.Features.Pools.Entities;
using BBMB.BudgetService.Features.Pools.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Pools.GetList
{
    public class GetPoolListHandler : IRequestHandler<GetPoolListQuery, PagedResponse<GetPoolListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetPoolListHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<GetPoolListResponseDto>> Handle(GetPoolListQuery request, CancellationToken ct)
        {
            var query = _context.Pools.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.PoolCode))
                query = query.Where(p => p.PoolCode.Contains(request.PoolCode));

            if (!string.IsNullOrEmpty(request.PoolDesc))
                query = query.Where(p => p.PoolDesc.Contains(request.PoolDesc));

            if (!string.IsNullOrEmpty(request.UserCode))
                query = query.Where(p => p.UserCode == request.UserCode);

            if (request.ModificationDateFrom.HasValue)
                query = query.Where(p => p.ModificationDate >= request.ModificationDateFrom.Value);

            if (request.ModificationDateTo.HasValue)
                query = query.Where(p => p.ModificationDate <= request.ModificationDateTo.Value);

            // Get total count
            var totalCount = await query.CountAsync(ct);

            // Apply sorting
            query = ApplySorting(query, request.SortBy, request.SortDescending);

            // Apply pagination
            var pageSize = Math.Min(request.PageSize, 100);
            var items = await query
                .Skip((request.PageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new GetPoolListResponseDto
                {
                    PoolCode = p.PoolCode,
                    PoolDesc = p.PoolDesc,
                    ModificationDate = p.ModificationDate,
                    UserCode = p.UserCode
                })
                .ToListAsync(ct);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResponse<GetPoolListResponseDto>
            {
                Message = "Pools retrieved successfully",
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

        private IQueryable<Pool> ApplySorting(IQueryable<Pool> query, string? sortBy, bool sortDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(p => p.PoolCode);

            return sortBy.ToLower() switch
            {
                "poolcode" => sortDescending ? query.OrderByDescending(p => p.PoolCode) : query.OrderBy(p => p.PoolCode),
                "pooldesc" => sortDescending ? query.OrderByDescending(p => p.PoolDesc) : query.OrderBy(p => p.PoolDesc),
                "modificationdate" => sortDescending ? query.OrderByDescending(p => p.ModificationDate) : query.OrderBy(p => p.ModificationDate),
                "usercode" => sortDescending ? query.OrderByDescending(p => p.UserCode) : query.OrderBy(p => p.UserCode),
                _ => query.OrderBy(p => p.PoolCode)
            };
        }
    }
}
