using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.SubMinor.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinor.GetList
{
    public class GetSubMinorHeadListHandler : IRequestHandler<GetSubMinorHeadListQuery, PagedResponse<GetSubMinorHeadListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetSubMinorHeadListHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<GetSubMinorHeadListResponseDto>> Handle(GetSubMinorHeadListQuery request, CancellationToken ct)
        {
            var query = _context.SubMinorHeads
                //.Include(s => s.MajorHead)
                //.Include(s => s.SubMajorHead)
                .Include(s => s.MinorHead)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.SubMinorAccCode))
                query = query.Where(s => s.SubMinorAccCode == request.SubMinorAccCode);

            //if (!string.IsNullOrEmpty(request.MajorAccCode))
            //    query = query.Where(s => s.MajorAccCode == request.MajorAccCode);

            //if (!string.IsNullOrEmpty(request.SubMajorAccCode))
            //    query = query.Where(s => s.SubMajorAccCode == request.SubMajorAccCode);

            if (!string.IsNullOrEmpty(request.MinorAccCode))
                query = query.Where(s => s.MinorAccCode == request.MinorAccCode);

            if (!string.IsNullOrEmpty(request.SubMinorAccDesc))
                query = query.Where(s => s.SubMinorAccDesc.Contains(request.SubMinorAccDesc));

            if (!string.IsNullOrEmpty(request.UserCode))
                query = query.Where(s => s.UserCode == request.UserCode);

            if (request.ModificationDateFrom.HasValue)
                query = query.Where(s => s.ModificationDate >= request.ModificationDateFrom.Value);

            if (request.ModificationDateTo.HasValue)
                query = query.Where(s => s.ModificationDate <= request.ModificationDateTo.Value);

            // Get total count
            var totalCount = await query.CountAsync(ct);

            // Apply sorting
            query = ApplySorting(query, request.SortBy, request.SortDescending);

            // Apply pagination
            var pageSize = Math.Min(request.PageSize, 100);
            var items = await query
                .Skip((request.PageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new GetSubMinorHeadListResponseDto
                {
                    SubMinorAccCode = s.SubMinorAccCode,
                    SubMinorAccDesc = s.SubMinorAccDesc,
                    //MajorAccCode = s.MajorAccCode,
                    //MajorAccDesc = s.MajorHead.MajorAccDesc,
                    //SubMajorAccCode = s.SubMajorAccCode,
                    //SubMajorAccDesc = s.SubMajorHead.SubMajorAccDesc,
                    MinorAccCode = s.MinorAccCode,
                    MinorAccDesc = s.MinorHead.MinorAccDesc,
                    ModificationDate = s.ModificationDate,
                    UserCode = s.UserCode
                })
                .ToListAsync(ct);

            return new PagedResponse<GetSubMinorHeadListResponseDto>
            {
                Message = "Sub-minor heads retrieved successfully",
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

        private IQueryable<SubMinorHead> ApplySorting(IQueryable<SubMinorHead> query, string? sortBy, bool sortDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(s => s.SubMinorAccCode);

            return sortBy.ToLower() switch
            {
                "subminoracccode" => sortDescending ? query.OrderByDescending(s => s.SubMinorAccCode) : query.OrderBy(s => s.SubMinorAccCode),
                //"majoracccode" => sortDescending ? query.OrderByDescending(s => s.MajorAccCode) : query.OrderBy(s => s.MajorAccCode),
                //"submajoracccode" => sortDescending ? query.OrderByDescending(s => s.SubMajorAccCode) : query.OrderBy(s => s.SubMajorAccCode),
                "minoracccode" => sortDescending ? query.OrderByDescending(s => s.MinorAccCode) : query.OrderBy(s => s.MinorAccCode),
                "subminoraccdesc" => sortDescending ? query.OrderByDescending(s => s.SubMinorAccDesc) : query.OrderBy(s => s.SubMinorAccDesc),
                "modificationdate" => sortDescending ? query.OrderByDescending(s => s.ModificationDate) : query.OrderBy(s => s.ModificationDate),
                _ => query.OrderBy(s => s.SubMinorAccCode)
            };
        }
    }
}
