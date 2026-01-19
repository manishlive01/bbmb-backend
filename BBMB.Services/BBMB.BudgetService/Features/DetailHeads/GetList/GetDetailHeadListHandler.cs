using BBMB.BudgetService.Features.DetailHeads.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.DetailHeads.GetList
{
    public class GetDetailHeadListHandler : IRequestHandler<GetDetailHeadListQuery, PagedResponse<GetDetailHeadListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetDetailHeadListHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<GetDetailHeadListResponseDto>> Handle(GetDetailHeadListQuery request, CancellationToken cancellationToken)
        {
            // Start with base query including all parent entities
            var query = _context.DetailHeads
                .Include(d => d.MajorHead)
                .Include(d => d.SubMajorHead)
                .Include(d => d.MinorHead)
                .Include(d => d.SubMinorHead)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(request.DetailAccCode))
            {
                query = query.Where(d => d.DetailAccCode.Contains(request.DetailAccCode));
            }

            if (!string.IsNullOrWhiteSpace(request.MajorAccCode))
            {
                query = query.Where(d => d.MajorAccCode == request.MajorAccCode);
            }

            if (!string.IsNullOrWhiteSpace(request.SubMajorAccCode))
            {
                query = query.Where(d => d.SubMajorAccCode == request.SubMajorAccCode);
            }

            if (!string.IsNullOrWhiteSpace(request.MinorAccCode))
            {
                query = query.Where(d => d.MinorAccCode == request.MinorAccCode);
            }

            if (!string.IsNullOrWhiteSpace(request.SubMinorAccCode))
            {
                query = query.Where(d => d.SubMinorAccCode == request.SubMinorAccCode);
            }

            if (!string.IsNullOrWhiteSpace(request.DetailAccDesc))
            {
                query = query.Where(d => d.DetailAccDesc.Contains(request.DetailAccDesc));
            }

            if (!string.IsNullOrWhiteSpace(request.UserCode))
            {
                query = query.Where(d => d.UserCode == request.UserCode);
            }

            if (request.ModificationDateFrom.HasValue)
            {
                query = query.Where(d => d.ModificationDate >= request.ModificationDateFrom.Value);
            }

            if (request.ModificationDateTo.HasValue)
            {
                query = query.Where(d => d.ModificationDate <= request.ModificationDateTo.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = ApplySorting(query, request.SortBy, request.SortDescending);

            // Apply pagination with max page size of 100
            var pageSize = Math.Min(request.PageSize, 100);
            var pageNumber = Math.Max(request.PageNumber, 1);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new GetDetailHeadListResponseDto
                {
                    DetailAccCode = d.DetailAccCode,
                    DetailAccDesc = d.DetailAccDesc,
                    MajorAccCode = d.MajorAccCode,
                    MajorAccDesc = d.MajorHead != null ? d.MajorHead.MajorAccDesc : string.Empty,
                    SubMajorAccCode = d.SubMajorAccCode,
                    SubMajorAccDesc = d.SubMajorHead != null ? d.SubMajorHead.SubMajorAccDesc : string.Empty,
                    MinorAccCode = d.MinorAccCode,
                    MinorAccDesc = d.MinorHead != null ? d.MinorHead.MinorAccDesc : string.Empty,
                    SubMinorAccCode = d.SubMinorAccCode,
                    SubMinorAccDesc = d.SubMinorHead != null ? d.SubMinorHead.SubMinorAccDesc : string.Empty,
                    UserCode = d.UserCode,
                    ModificationDate = d.ModificationDate
                })
                .ToListAsync(cancellationToken);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var paginationMetadata = new PaginationMetadata
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages
            };

            return new PagedResponse<GetDetailHeadListResponseDto>
            {
                Message = "Detail heads retrieved successfully",
                Data = items,
                Pagination = paginationMetadata
            };
        }

        private IQueryable<BBMB.BudgetService.Features.DetailHeads.Entities.DetailHead> ApplySorting(
            IQueryable<BBMB.BudgetService.Features.DetailHeads.Entities.DetailHead> query,
            string? sortBy,
            bool sortDescending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                return query.OrderBy(d => d.DetailAccCode);
            }

            query = sortBy.ToLower() switch
            {
                "detailacccode" => sortDescending ? query.OrderByDescending(d => d.DetailAccCode) : query.OrderBy(d => d.DetailAccCode),
                "detailaccdesc" => sortDescending ? query.OrderByDescending(d => d.DetailAccDesc) : query.OrderBy(d => d.DetailAccDesc),
                "majoracccode" => sortDescending ? query.OrderByDescending(d => d.MajorAccCode) : query.OrderBy(d => d.MajorAccCode),
                "submajoracccode" => sortDescending ? query.OrderByDescending(d => d.SubMajorAccCode) : query.OrderBy(d => d.SubMajorAccCode),
                "minoracccode" => sortDescending ? query.OrderByDescending(d => d.MinorAccCode) : query.OrderBy(d => d.MinorAccCode),
                "subminoracccode" => sortDescending ? query.OrderByDescending(d => d.SubMinorAccCode) : query.OrderBy(d => d.SubMinorAccCode),
                "usercode" => sortDescending ? query.OrderByDescending(d => d.UserCode) : query.OrderBy(d => d.UserCode),
                "modificationdate" => sortDescending ? query.OrderByDescending(d => d.ModificationDate) : query.OrderBy(d => d.ModificationDate),
                _ => query.OrderBy(d => d.DetailAccCode)
            };

            return query;
        }
    }
}
