using BBMB.BudgetService.Features.Regions.Entities;
using BBMB.BudgetService.Features.Regions.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Regions.GetList
{
    public class GetRegionListHandler : IRequestHandler<GetRegionListQuery, PagedResponse<GetRegionListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetRegionListHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<GetRegionListResponseDto>> Handle(GetRegionListQuery request, CancellationToken ct)
        {
            var query = _context.Regions.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.RegionCode))
                query = query.Where(r => r.RegionCode.Contains(request.RegionCode));

            if (!string.IsNullOrEmpty(request.RegionDesc))
                query = query.Where(r => r.RegionDesc.Contains(request.RegionDesc));

            if (!string.IsNullOrEmpty(request.UserCode))
                query = query.Where(r => r.UserCode == request.UserCode);

            if (request.ModificationDateFrom.HasValue)
                query = query.Where(r => r.ModificationDate >= request.ModificationDateFrom.Value);

            if (request.ModificationDateTo.HasValue)
                query = query.Where(r => r.ModificationDate <= request.ModificationDateTo.Value);

            // Get total count
            var totalCount = await query.CountAsync(ct);

            // Apply sorting
            query = ApplySorting(query, request.SortBy, request.SortDescending);

            // Apply pagination
            var pageSize = Math.Min(request.PageSize, 100);
            var items = await query
                .Skip((request.PageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new GetRegionListResponseDto
                {
                    RegionCode = r.RegionCode,
                    RegionDesc = r.RegionDesc,
                    ModificationDate = r.ModificationDate,
                    UserCode = r.UserCode
                })
                .ToListAsync(ct);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResponse<GetRegionListResponseDto>
            {
                Message = "Regions retrieved successfully",
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

        private IQueryable<Region> ApplySorting(IQueryable<Region> query, string? sortBy, bool sortDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(r => r.RegionCode);

            return sortBy.ToLower() switch
            {
                "regioncode" => sortDescending ? query.OrderByDescending(r => r.RegionCode) : query.OrderBy(r => r.RegionCode),
                "regiondesc" => sortDescending ? query.OrderByDescending(r => r.RegionDesc) : query.OrderBy(r => r.RegionDesc),
                "modificationdate" => sortDescending ? query.OrderByDescending(r => r.ModificationDate) : query.OrderBy(r => r.ModificationDate),
                "usercode" => sortDescending ? query.OrderByDescending(r => r.UserCode) : query.OrderBy(r => r.UserCode),
                _ => query.OrderBy(r => r.RegionCode)
            };
        }
    }
}
