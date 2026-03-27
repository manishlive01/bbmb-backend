using BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Entites;
using BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.GetList
{
    public class GetCRSectionManagementListHandler
        : IRequestHandler<GetCRSectionManagementListQuery, PagedResponse<GetCRSectionManagementListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetCRSectionManagementListHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<GetCRSectionManagementListResponseDto>> Handle(
            GetCRSectionManagementListQuery request,
            CancellationToken ct)
        {
            var query = _context.Set<CRSection>()
                .Include(x => x.CRManagement) // ✅ navigation
                .AsQueryable();

            // 🔎 Filters
            if (request.CRId.HasValue)
                query = query.Where(x => x.CRId == request.CRId.Value);

            if (request.CRCode.HasValue)
                query = query.Where(x => x.CRManagement.CRCode == request.CRCode.Value);

            if (!string.IsNullOrWhiteSpace(request.CRName))
                query = query.Where(x => x.CRManagement.Name.Contains(request.CRName));

            if (request.Status.HasValue)
                query = query.Where(x => x.Status == request.Status.Value);

            // 📊 Count
            var totalCount = await query.CountAsync(ct);

            // 🔃 Sorting
            query = ApplySorting(query, request.SortBy, request.SortDescending);

            // 📄 Pagination
            var pageSize = Math.Min(request.PageSize, 100);

            var list = await query
                .Skip((request.PageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            var items = list.Select(x => new GetCRSectionManagementListResponseDto
            {
                Id = x.Id,
                CRId = x.CRId,
                CRCode = x.CRManagement.CRCode,   // ✅ from navigation
                CRName = x.CRManagement.Name,     // ✅
                CRSectionCode = x.CRSectionCode,
                Name = x.Name,
                Description = x.Description,
                Location = x.Location,
                Date = x.Date,
                Status = x.Status
            }).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResponse<GetCRSectionManagementListResponseDto>
            {
                Message = "CR Section records retrieved successfully",
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

        private IQueryable<CRSection> ApplySorting(
            IQueryable<CRSection> query,
            string? sortBy,
            bool sortDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(x => x.Id);

            return sortBy.ToLower() switch
            {
                "name" => sortDescending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
                "crname" => sortDescending ? query.OrderByDescending(x => x.CRManagement.Name) : query.OrderBy(x => x.CRManagement.Name),
                "crcode" => sortDescending ? query.OrderByDescending(x => x.CRManagement.CRCode) : query.OrderBy(x => x.CRManagement.CRCode),
                "date" => sortDescending ? query.OrderByDescending(x => x.Date) : query.OrderBy(x => x.Date),
                "status" => sortDescending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                _ => query.OrderBy(x => x.Id)
            };
        }
    }
}