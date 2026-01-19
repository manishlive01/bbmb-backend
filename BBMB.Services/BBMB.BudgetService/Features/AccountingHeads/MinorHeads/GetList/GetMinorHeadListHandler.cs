using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetList
{
    public class GetMinorHeadListHandler : IRequestHandler<GetMinorHeadListQuery, PagedResponse<GetMinorHeadListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetMinorHeadListHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<GetMinorHeadListResponseDto>> Handle(GetMinorHeadListQuery request, CancellationToken ct)
        {
            var query = _context.MinorHeads
                .Include(m => m.MajorHead)
                .Include(m => m.SubMajorHead)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.MinorAccCode))
                query = query.Where(m => m.MinorAccCode == request.MinorAccCode);

            if (!string.IsNullOrEmpty(request.MajorAccCode))
                query = query.Where(m => m.MajorAccCode == request.MajorAccCode);

            if (!string.IsNullOrEmpty(request.SubMajorAccCode))
                query = query.Where(m => m.SubMajorAccCode == request.SubMajorAccCode);

            if (!string.IsNullOrEmpty(request.MinorAccDesc))
                query = query.Where(m => m.MinorAccDesc.Contains(request.MinorAccDesc));

            if (!string.IsNullOrEmpty(request.UserCode))
                query = query.Where(m => m.UserCode == request.UserCode);

            if (request.ModificationDateFrom.HasValue)
                query = query.Where(m => m.ModificationDate >= request.ModificationDateFrom.Value);

            if (request.ModificationDateTo.HasValue)
                query = query.Where(m => m.ModificationDate <= request.ModificationDateTo.Value);

            // Get total count
            var totalCount = await query.CountAsync(ct);

            // Apply sorting
            query = ApplySorting(query, request.SortBy, request.SortDescending);

            // Apply pagination
            var pageSize = Math.Min(request.PageSize, 100);
            var items = await query
                .Skip((request.PageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new GetMinorHeadListResponseDto
                {
                    MinorAccCode = m.MinorAccCode,
                    MinorAccDesc = m.MinorAccDesc,
                    MajorAccCode = m.MajorAccCode,
                    MajorAccDesc = m.MajorHead.MajorAccDesc,
                    SubMajorAccCode = m.SubMajorAccCode,
                    SubMajorAccDesc = m.SubMajorHead.SubMajorAccDesc,
                    ModificationDate = m.ModificationDate,
                    UserCode = m.UserCode
                })
                .ToListAsync(ct);

            return new PagedResponse<GetMinorHeadListResponseDto>
            {
                Message = "Minor heads retrieved successfully",
                Data = items,
                Pagination = new PaginationMetadata
                {
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    HasPreviousPage = request.PageNumber > 1,
                    HasNextPage = request.PageNumber < (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            };
        }

        private IQueryable<MinorHead> ApplySorting(IQueryable<MinorHead> query, string? sortBy, bool sortDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(m => m.MinorAccCode);

            return sortBy.ToLower() switch
            {
                "minoracccode" => sortDescending ? query.OrderByDescending(m => m.MinorAccCode) : query.OrderBy(m => m.MinorAccCode),
                "majoracccode" => sortDescending ? query.OrderByDescending(m => m.MajorAccCode) : query.OrderBy(m => m.MajorAccCode),
                "submajoracccode" => sortDescending ? query.OrderByDescending(m => m.SubMajorAccCode) : query.OrderBy(m => m.SubMajorAccCode),
                "minoraccdesc" => sortDescending ? query.OrderByDescending(m => m.MinorAccDesc) : query.OrderBy(m => m.MinorAccDesc),
                "modificationdate" => sortDescending ? query.OrderByDescending(m => m.ModificationDate) : query.OrderBy(m => m.ModificationDate),
                _ => query.OrderBy(m => m.MinorAccCode)
            };
        }
    }
}
