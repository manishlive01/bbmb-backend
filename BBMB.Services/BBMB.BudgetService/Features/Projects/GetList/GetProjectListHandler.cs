using BBMB.BudgetService.Features.Projects.Entities;
using BBMB.BudgetService.Features.Projects.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Projects.GetList
{
    public class GetProjectListHandler : IRequestHandler<GetProjectListQuery, PagedResponse<GetProjectListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetProjectListHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<GetProjectListResponseDto>> Handle(GetProjectListQuery request, CancellationToken ct)
        {
            var query = _context.Projects.Include(p => p.UnitMasters).AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.ProjCode))
                query = query.Where(p => p.ProjCode.Contains(request.ProjCode));

            if (!string.IsNullOrEmpty(request.ProjDesc))
                query = query.Where(p => p.ProjDesc.Contains(request.ProjDesc));

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
                .Select(p => new GetProjectListResponseDto
                {
                    ProjCode = p.ProjCode,
                    ProjDesc = p.ProjDesc,
                    ModificationDate = p.ModificationDate,
                    UserCode = p.UserCode,
                    AssociatedUnitsCount = p.UnitMasters.Count
                })
                .ToListAsync(ct);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResponse<GetProjectListResponseDto>
            {
                Message = "Projects retrieved successfully",
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

        private IQueryable<Project> ApplySorting(IQueryable<Project> query, string? sortBy, bool sortDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(p => p.ProjCode);

            return sortBy.ToLower() switch
            {
                "projcode" => sortDescending ? query.OrderByDescending(p => p.ProjCode) : query.OrderBy(p => p.ProjCode),
                "projdesc" => sortDescending ? query.OrderByDescending(p => p.ProjDesc) : query.OrderBy(p => p.ProjDesc),
                "modificationdate" => sortDescending ? query.OrderByDescending(p => p.ModificationDate) : query.OrderBy(p => p.ModificationDate),
                "usercode" => sortDescending ? query.OrderByDescending(p => p.UserCode) : query.OrderBy(p => p.UserCode),
                _ => query.OrderBy(p => p.ProjCode)
            };
        }
    }
}
