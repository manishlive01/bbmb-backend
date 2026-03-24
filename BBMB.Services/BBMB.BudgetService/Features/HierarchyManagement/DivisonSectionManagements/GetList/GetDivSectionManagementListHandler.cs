using BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.Entities;
using BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.GetList
{
    public class GetDivSectionManagementListHandler
        : IRequestHandler<GetDivSectionManagementListQuery, PagedResponse<GetDivSectionManagementListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetDivSectionManagementListHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<GetDivSectionManagementListResponseDto>> Handle(
            GetDivSectionManagementListQuery request,
            CancellationToken ct)
        {
            var query = _context.Set<DivSection>()
                .Include(x => x.Division)   // ✅ navigation
                .AsQueryable();

            // 🔎 Filters
            if (request.DivisionId.HasValue)
                query = query.Where(x => x.DivisionId == request.DivisionId.Value);

            if (request.DivisionCode.HasValue)
                query = query.Where(x => x.Division.DivisionCode == request.DivisionCode.Value);

            if (!string.IsNullOrWhiteSpace(request.DivisionName))
                query = query.Where(x => x.Division.DivisionDesc.Contains(request.DivisionName));

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

            var items = list.Select(x => new GetDivSectionManagementListResponseDto
            {
                Id = x.Id,
                DivisionId = x.DivisionId,
                DivisionCode = x.Division.DivisionCode,
                DivisionName = x.Division.DivisionDesc,
                DivSectionCode = x.DivSectionCode,
                Name = x.Name,
                Description = x.Description,
                Location = x.Location,
                Date = x.Date,
                Status = x.Status
            }).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResponse<GetDivSectionManagementListResponseDto>
            {
                Message = "Division Section records retrieved successfully",
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

        private IQueryable<DivSection> ApplySorting(
            IQueryable<DivSection> query,
            string? sortBy,
            bool sortDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(x => x.DivSectionCode);

            return sortBy.ToLower() switch
            {
                "name" => sortDescending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
                "divisionname" => sortDescending ? query.OrderByDescending(x => x.Division.DivisionDesc) : query.OrderBy(x => x.Division.DivisionDesc),
                "divisioncode" => sortDescending ? query.OrderByDescending(x => x.Division.DivisionCode) : query.OrderBy(x => x.Division.DivisionCode),
                "date" => sortDescending ? query.OrderByDescending(x => x.Date) : query.OrderBy(x => x.Date),
                "status" => sortDescending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                _ => query.OrderBy(x => x.DivSectionCode)
            };
        }
    }
}