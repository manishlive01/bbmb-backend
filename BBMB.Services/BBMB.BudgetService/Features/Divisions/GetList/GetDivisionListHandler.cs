using BBMB.BudgetService.Features.Divisions.Entities;
using BBMB.BudgetService.Features.Divisions.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Divisions.GetList
{
    public class GetDivisionListHandler : IRequestHandler<GetDivisionListQuery, PagedResponse<GetDivisionListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetDivisionListHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<GetDivisionListResponseDto>> Handle(GetDivisionListQuery request, CancellationToken ct)
        {
            var query = _context.Divisions.AsQueryable();

            // Apply filters
            if (request.DivisionCode.HasValue)
                query = query.Where(d => d.DivisionCode == request.DivisionCode.Value);

            if (!string.IsNullOrEmpty(request.DivisionDesc))
                query = query.Where(d => d.DivisionDesc.Contains(request.DivisionDesc));

            if (!string.IsNullOrEmpty(request.UserCode))
                query = query.Where(d => d.UserCode == request.UserCode);

            if (!string.IsNullOrEmpty(request.RegionCode))
                query = query.Where(d => d.RegionCode == request.RegionCode);

            if (!string.IsNullOrEmpty(request.PoolCode))
                query = query.Where(d => d.PoolCode == request.PoolCode);

            if (request.ModificationDateFrom.HasValue)
                query = query.Where(d => d.ModificationDate >= request.ModificationDateFrom.Value);

            if (request.ModificationDateTo.HasValue)
                query = query.Where(d => d.ModificationDate <= request.ModificationDateTo.Value);

            // Get total count
            var totalCount = await query.CountAsync(ct);

            // Apply sorting
            query = ApplySorting(query, request.SortBy, request.SortDescending);

            // Apply pagination
            var pageSize = Math.Min(request.PageSize, 100);
            var items = await query
                .Skip((request.PageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new GetDivisionListResponseDto
                {
                    DivisionCode = d.DivisionCode,
                    DivSection = d.DivSection,
                    DivisionDesc = d.DivisionDesc,
                    RegionCode = d.RegionCode,
                    PoolCode = d.PoolCode,
                    ModificationDate = d.ModificationDate,
                    UserCode = d.UserCode
                })
                .ToListAsync(ct);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResponse<GetDivisionListResponseDto>
            {
                Message = "Divisions retrieved successfully",
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

        private IQueryable<Division> ApplySorting(IQueryable<Division> query, string? sortBy, bool sortDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(d => d.DivisionCode);

            return sortBy.ToLower() switch
            {
                "divisioncode" => sortDescending ? query.OrderByDescending(d => d.DivisionCode) : query.OrderBy(d => d.DivisionCode),
                "divisiondesc" => sortDescending ? query.OrderByDescending(d => d.DivisionDesc) : query.OrderBy(d => d.DivisionDesc),
                "modificationdate" => sortDescending ? query.OrderByDescending(d => d.ModificationDate) : query.OrderBy(d => d.ModificationDate),
                _ => query.OrderBy(d => d.DivisionCode)
            };
        }
    }
}
