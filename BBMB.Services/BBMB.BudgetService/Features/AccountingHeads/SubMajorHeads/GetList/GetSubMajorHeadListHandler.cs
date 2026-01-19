using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetList
{
    public class GetSubMajorHeadListHandler : IRequestHandler<GetSubMajorHeadListQuery, PagedResponse<GetSubMajorHeadListResponseDto>>
    {
        private readonly BudgetContext _context;
        
        public GetSubMajorHeadListHandler(BudgetContext context)
        {
            _context = context;
        }
        
        public async Task<PagedResponse<GetSubMajorHeadListResponseDto>> Handle(GetSubMajorHeadListQuery request, CancellationToken ct)
        {
            var query = _context.SubMajorHeads
                .Include(s => s.MajorHead)
                .AsQueryable();
            
            // Apply filters
            if (!string.IsNullOrEmpty(request.SubMajorAccCode))
                query = query.Where(s => s.SubMajorAccCode == request.SubMajorAccCode);
                
            if (!string.IsNullOrEmpty(request.MajorAccCode))
                query = query.Where(s => s.MajorAccCode == request.MajorAccCode);
                
            if (!string.IsNullOrEmpty(request.SubMajorAccDesc))
                query = query.Where(s => s.SubMajorAccDesc.Contains(request.SubMajorAccDesc));
                
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
                .Select(s => new GetSubMajorHeadListResponseDto
                {
                    SubMajorAccCode = s.SubMajorAccCode,
                    SubMajorAccDesc = s.SubMajorAccDesc,
                    MajorAccCode = s.MajorAccCode,
                    MajorAccDesc = s.MajorHead.MajorAccDesc,
                    ModificationDate = s.ModificationDate,
                    UserCode = s.UserCode
                })
                .ToListAsync(ct);
            
            return new PagedResponse<GetSubMajorHeadListResponseDto>
            {
                Message = "Sub-major heads retrieved successfully",
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
        
        private IQueryable<SubMajorHead> ApplySorting(IQueryable<SubMajorHead> query, string? sortBy, bool sortDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(s => s.SubMajorAccCode);
                
            return sortBy.ToLower() switch
            {
                "submajoracccode" => sortDescending ? query.OrderByDescending(s => s.SubMajorAccCode) : query.OrderBy(s => s.SubMajorAccCode),
                "majoracccode" => sortDescending ? query.OrderByDescending(s => s.MajorAccCode) : query.OrderBy(s => s.MajorAccCode),
                "submajoraccdesc" => sortDescending ? query.OrderByDescending(s => s.SubMajorAccDesc) : query.OrderBy(s => s.SubMajorAccDesc),
                "modificationdate" => sortDescending ? query.OrderByDescending(s => s.ModificationDate) : query.OrderBy(s => s.ModificationDate),
                _ => query.OrderBy(s => s.SubMajorAccCode)
            };
        }
    }
}
