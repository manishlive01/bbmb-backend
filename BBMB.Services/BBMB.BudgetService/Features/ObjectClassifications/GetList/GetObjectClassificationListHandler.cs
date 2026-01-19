using BBMB.BudgetService.Features.ObjectClassifications.Entities;
using BBMB.BudgetService.Features.ObjectClassifications.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.ObjectClassifications.GetList
{
    public class GetObjectClassificationListHandler : IRequestHandler<GetObjectClassificationListQuery, PagedResponse<GetObjectClassificationListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetObjectClassificationListHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<GetObjectClassificationListResponseDto>> Handle(GetObjectClassificationListQuery request, CancellationToken ct)
        {
            var query = _context.ObjectClassifications.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.ObjectAccCode))
                query = query.Where(o => o.ObjectAccCode == request.ObjectAccCode);

            if (!string.IsNullOrEmpty(request.ObjectAccDesc))
                query = query.Where(o => o.ObjectAccDesc.Contains(request.ObjectAccDesc));

            if (!string.IsNullOrEmpty(request.UserCode))
                query = query.Where(o => o.UserCode == request.UserCode);

            if (request.ModificationDateFrom.HasValue)
                query = query.Where(o => o.ModificationDate >= request.ModificationDateFrom.Value);

            if (request.ModificationDateTo.HasValue)
                query = query.Where(o => o.ModificationDate <= request.ModificationDateTo.Value);

            // Get total count
            var totalCount = await query.CountAsync(ct);

            // Apply sorting
            query = ApplySorting(query, request.SortBy, request.SortDescending);

            // Apply pagination
            var pageSize = Math.Min(request.PageSize, 100);
            var items = await query
                .Skip((request.PageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new GetObjectClassificationListResponseDto
                {
                    ObjectAccCode = o.ObjectAccCode,
                    ObjectAccDesc = o.ObjectAccDesc,
                    ModificationDate = o.ModificationDate,
                    UserCode = o.UserCode
                })
                .ToListAsync(ct);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResponse<GetObjectClassificationListResponseDto>
            {
                Message = "Object classifications retrieved successfully",
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

        private IQueryable<ObjectClassification> ApplySorting(IQueryable<ObjectClassification> query, string? sortBy, bool sortDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(o => o.ObjectAccCode);

            return sortBy.ToLower() switch
            {
                "objectacccode" => sortDescending ? query.OrderByDescending(o => o.ObjectAccCode) : query.OrderBy(o => o.ObjectAccCode),
                "objectaccdesc" => sortDescending ? query.OrderByDescending(o => o.ObjectAccDesc) : query.OrderBy(o => o.ObjectAccDesc),
                "modificationdate" => sortDescending ? query.OrderByDescending(o => o.ModificationDate) : query.OrderBy(o => o.ModificationDate),
                "usercode" => sortDescending ? query.OrderByDescending(o => o.UserCode) : query.OrderBy(o => o.UserCode),
                _ => query.OrderBy(o => o.ObjectAccCode)
            };
        }
    }
}
