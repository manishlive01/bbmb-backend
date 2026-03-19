using BBMB.BudgetService.Features.HierarchyManagement.CEManagements.Entities;
using BBMB.BudgetService.Features.HierarchyManagement.CEManagements.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.HierarchyManagement.CEManagements.GetList
{
    public class GetCEManagementListHandler
        : IRequestHandler<GetCEManagementListQuery, PagedResponse<GetCEManagementListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetCEManagementListHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<GetCEManagementListResponseDto>> Handle(
            GetCEManagementListQuery request,
            CancellationToken ct)
        {
            var query = _context.CEManagements.AsQueryable();

            // 🔎 Apply Filters
            if (!string.IsNullOrEmpty(request.ProjectCode))
                query = query.Where(x => x.ProjectCode == request.ProjectCode);

            if (request.Status.HasValue)
                query = query.Where(x => x.Status == request.Status.Value);

            // 📊 Total Count
            var totalCount = await query.CountAsync(ct);

            // 🔃 Sorting
            query = ApplySorting(query, request.SortBy, request.SortDescending);

            // 📄 Pagination
            var pageSize = Math.Min(request.PageSize, 100);

            var ceList = await query
                .Skip((request.PageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            var items = ceList.Select(x => new GetCEManagementListResponseDto
            {
                CECode = x.CECode,
                Name = x.Name,
                ProjectCode = x.ProjectCode,
                Description = x.Description,
                Location = x.Location,
                Date = x.Date,
                Status = x.Status,
                UserCode = x.UserCode
            }).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResponse<GetCEManagementListResponseDto>
            {
                Message = "CE Management records retrieved successfully",
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

        private IQueryable<CEManagement> ApplySorting(
            IQueryable<CEManagement> query,
            string? sortBy,
            bool sortDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(x => x.CECode);

            return sortBy.ToLower() switch
            {
                "name" => sortDescending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
                "projectcode" => sortDescending ? query.OrderByDescending(x => x.ProjectCode) : query.OrderBy(x => x.ProjectCode),
                "date" => sortDescending ? query.OrderByDescending(x => x.Date) : query.OrderBy(x => x.Date),
                "status" => sortDescending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                _ => query.OrderBy(x => x.CECode)
            };
        }
    }
}
