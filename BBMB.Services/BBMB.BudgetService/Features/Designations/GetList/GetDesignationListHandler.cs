using BBMB.BudgetService.Features.Designations.Entities;
using BBMB.BudgetService.Features.Designations.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Designations.GetList
{
    public class GetDesignationListHandler : IRequestHandler<GetDesignationListQuery, PagedResponse<GetDesignationListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetDesignationListHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<GetDesignationListResponseDto>> Handle(GetDesignationListQuery request, CancellationToken ct)
        {
            var query = _context.Designations.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.DesignationCode))
                query = query.Where(d => d.DesignationCode == request.DesignationCode);

            if (!string.IsNullOrEmpty(request.DesignationDesc))
                query = query.Where(d => d.DesignationDesc.Contains(request.DesignationDesc));

            if (!string.IsNullOrEmpty(request.UserCode))
                query = query.Where(d => d.UserCode == request.UserCode);

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
                .Select(d => new GetDesignationListResponseDto
                {
                    DesignationCode = d.DesignationCode,
                    DesignationDesc = d.DesignationDesc,
                    ModificationDate = d.ModificationDate,
                    UserCode = d.UserCode
                })
                .ToListAsync(ct);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResponse<GetDesignationListResponseDto>
            {
                Message = "Designations retrieved successfully",
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

        private IQueryable<Designation> ApplySorting(IQueryable<Designation> query, string? sortBy, bool sortDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(d => d.DesignationCode);

            return sortBy.ToLower() switch
            {
                "designationcode" => sortDescending ? query.OrderByDescending(d => d.DesignationCode) : query.OrderBy(d => d.DesignationCode),
                "designationdesc" => sortDescending ? query.OrderByDescending(d => d.DesignationDesc) : query.OrderBy(d => d.DesignationDesc),
                "modificationdate" => sortDescending ? query.OrderByDescending(d => d.ModificationDate) : query.OrderBy(d => d.ModificationDate),
                "usercode" => sortDescending ? query.OrderByDescending(d => d.UserCode) : query.OrderBy(d => d.UserCode),
                _ => query.OrderBy(d => d.DesignationCode)
            };
        }
    }
}
