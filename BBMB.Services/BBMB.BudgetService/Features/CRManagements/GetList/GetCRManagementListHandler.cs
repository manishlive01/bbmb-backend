using BBMB.BudgetService.Features.CRManagements.Entities;
using BBMB.BudgetService.Features.CRManagements.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.CRManagements.GetList
{
    public class GetCRManagementListHandler
        : IRequestHandler<GetCRManagementListQuery, PagedResponse<GetCRManagementListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetCRManagementListHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<GetCRManagementListResponseDto>> Handle(
            GetCRManagementListQuery request,
            CancellationToken ct)
        {
            var query = _context.CRManagements
                .Include(x => x.CEManagement)
                .AsQueryable();

            // 🔎 Apply Filters
            if (request.ceCode.HasValue)
            {
                query = query.Where(x => x.CECode == request.ceCode.Value);
            }

            if (request.Status.HasValue)
                query = query.Where(x => x.Status == request.Status.Value);

            // 📊 Total Count
            var totalCount = await query.CountAsync(ct);

            // 🔃 Sorting
            query = ApplySorting(query, request.SortBy, request.SortDescending);

            // 📄 Pagination
            var pageSize = Math.Min(request.PageSize, 100);

            var crList = await query
                .Skip((request.PageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            var items = crList.Select(x => new GetCRManagementListResponseDto
            {
                CRCode = x.CRCode,
                CECode = x.CECode,
                Name = x.Name,
                Description = x.Description,
                Location = x.Location,
                Date = x.Date,
                Status = x.Status,
                UserCode = x.UserCode
            }).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResponse<GetCRManagementListResponseDto>
            {
                Message = "CR Management records retrieved successfully",
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

        private IQueryable<CRManagement> ApplySorting(
            IQueryable<CRManagement> query,
            string? sortBy,
            bool sortDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(x => x.CRCode);

            return sortBy.ToLower() switch
            {
                "name" => sortDescending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
                "ceCode" => sortDescending ? query.OrderByDescending(x => x.CECode) : query.OrderBy(x => x.CECode),
                "crCode" => sortDescending ? query.OrderByDescending(x => x.CRCode) : query.OrderBy(x => x.CRCode),
                "date" => sortDescending ? query.OrderByDescending(x => x.Date) : query.OrderBy(x => x.Date),
                "status" => sortDescending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                _ => query.OrderBy(x => x.CRCode)
            };
        }
    }
}
