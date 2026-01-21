using BBMB.BudgetService.Features.Designations.Entities;
using BBMB.BudgetService.Features.Designations.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Designations.GetList
{
    public class GetDesignationListHandler
        : IRequestHandler<GetDesignationListCommand, PagedResponse<GetDesignationListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetDesignationListHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<GetDesignationListResponseDto>> Handle(
            GetDesignationListCommand request,
            CancellationToken ct)
        {
            var r = request.Request;

            var query = _context.Designations
                .AsNoTracking()
                .AsQueryable();

            // ===== Filters =====
            if (!string.IsNullOrWhiteSpace(r.DesignationCode))
                query = query.Where(d => d.DesignationCode == r.DesignationCode.Trim());

            if (!string.IsNullOrWhiteSpace(r.DesignationDesc))
                query = query.Where(d => d.DesignationDesc.Contains(r.DesignationDesc.Trim()));

            if (!string.IsNullOrWhiteSpace(r.UserCode))
                query = query.Where(d => d.UserCode == r.UserCode.Trim());

            if (r.ModificationDateFrom.HasValue)
                query = query.Where(d => d.ModificationDate >= r.ModificationDateFrom.Value);

            if (r.ModificationDateTo.HasValue)
                query = query.Where(d => d.ModificationDate <= r.ModificationDateTo.Value);

            // ===== Total Count =====
            var totalCount = await query.CountAsync(ct);

            // ===== Sorting =====
            query = ApplySorting(query, r.SortBy, r.SortDescending);

            // ===== Pagination =====
            var pageNumber = r.PageNumber <= 0 ? 1 : r.PageNumber;
            var pageSize = r.PageSize <= 0 ? 10 : Math.Min(r.PageSize, 100);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new GetDesignationListResponseDto
                {
                    DesignationCode = d.DesignationCode,
                    DesignationDesc = d.DesignationDesc,
                    ClassCode = d.ClassCode,
                    MinBasic = d.MinBasic,
                    MaxBasic = d.MaxBasic,
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
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    HasPreviousPage = pageNumber > 1,
                    HasNextPage = pageNumber < totalPages
                }
            };
        }

        private static IQueryable<Designation> ApplySorting(
            IQueryable<Designation> query,
            string? sortBy,
            bool sortDescending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return query.OrderBy(d => d.DesignationCode);

            return sortBy.Trim().ToLower() switch
            {
                "designationcode" =>
                    sortDescending ? query.OrderByDescending(d => d.DesignationCode) : query.OrderBy(d => d.DesignationCode),

                "designationdesc" =>
                    sortDescending ? query.OrderByDescending(d => d.DesignationDesc) : query.OrderBy(d => d.DesignationDesc),

                "modificationdate" =>
                    sortDescending ? query.OrderByDescending(d => d.ModificationDate) : query.OrderBy(d => d.ModificationDate),

                "usercode" =>
                    sortDescending ? query.OrderByDescending(d => d.UserCode) : query.OrderBy(d => d.UserCode),

                _ => query.OrderBy(d => d.DesignationCode)
            };
        }
    }
}
