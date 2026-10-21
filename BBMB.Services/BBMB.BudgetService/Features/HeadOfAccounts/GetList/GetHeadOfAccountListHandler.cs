using BBMB.BudgetService.Features.HeadOfAccounts.Entities;
using BBMB.BudgetService.Features.HeadOfAccounts.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.HeadOfAccounts.GetList
{
    public class GetHeadOfAccountListHandler : IRequestHandler<GetHeadOfAccountListQuery, PagedResponse<GetHeadOfAccountListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetHeadOfAccountListHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<GetHeadOfAccountListResponseDto>> Handle(
    GetHeadOfAccountListQuery request,
    CancellationToken ct)
        {
            var query = _context.HeadOfAccounts
                .Include(h => h.MajorHead)
                .Include(h => h.MinorHead)
                .Include(h => h.DetailHead)
                .Include(h => h.ObjectClassification)
                .AsQueryable();

            // Filters
            if (!string.IsNullOrEmpty(request.MajorAccCode))
                query = query.Where(h => h.MajorAccCode == request.MajorAccCode);

            if (!string.IsNullOrEmpty(request.SubMajorAccCode))
                query = query.Where(h => h.SubMajorAccCode == request.SubMajorAccCode);

            if (!string.IsNullOrEmpty(request.MinorAccCode))
                query = query.Where(h => h.MinorAccCode == request.MinorAccCode);

            if (!string.IsNullOrEmpty(request.SubMinorAccCode))
                query = query.Where(h => h.SubMinorAccCode == request.SubMinorAccCode);

            if (!string.IsNullOrEmpty(request.DetailAccCode))
                query = query.Where(h => h.DetailAccCode == request.DetailAccCode);

            if (!string.IsNullOrEmpty(request.ObjectAccCode))
                query = query.Where(h => h.ObjectAccCode == request.ObjectAccCode);

            if (!string.IsNullOrEmpty(request.HeadDesc))
                query = query.Where(h => h.HeadDesc.Contains(request.HeadDesc));

            if (request.ModificationDateFrom.HasValue)
                query = query.Where(h => h.ModificationDate >= request.ModificationDateFrom.Value);

            if (request.ModificationDateTo.HasValue)
                query = query.Where(h => h.ModificationDate <= request.ModificationDateTo.Value);

            var totalCount = await query.CountAsync(ct);

            query = ApplySorting(query, request.SortBy, request.SortDescending);

            var pageSize = Math.Min(request.PageSize, 100);

            var items = await query
                .Skip((request.PageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(h => new GetHeadOfAccountListResponseDto
                {
                    MajorAccCode = h.MajorAccCode,
                    SubMajorAccCode = h.SubMajorAccCode,
                    MinorAccCode = h.MinorAccCode,
                    SubMinorAccCode = h.SubMinorAccCode,
                    DetailAccCode = h.DetailAccCode,
                    ObjectAccCode = h.ObjectAccCode,

                    MajorHeadDescription = h.MajorHead != null
                        ? h.MajorHead.MajorAccDesc
                        : string.Empty,

                    MinorHeadDescription = h.MinorHead != null
                        ? h.MinorHead.MinorAccDesc
                        : string.Empty,

                    DetailHeadDescription = h.DetailHead != null
                        ? h.DetailHead.DetailAccDesc
                        : string.Empty,

                    ObjectClassificationDescription = h.ObjectClassification != null
                        ? h.ObjectClassification.ObjectAccDesc
                        : string.Empty,

                    HeadDesc = h.HeadDesc,
                    FullAccountCode = h.FullAccountCode,
                    AccountCodeId = h.AccountCodeId,
                    CompleteCode = h.CompleteCode,
                    NotForUse = h.NotForUse,
                    Origin = h.Origin,
                    ModificationDate = h.ModificationDate,
                    UserCode = h.UserCode
                })
                .ToListAsync(ct);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResponse<GetHeadOfAccountListResponseDto>
            {
                Message = "Head of accounts retrieved successfully",
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

        private IQueryable<HeadOfAccount> ApplySorting(
     IQueryable<HeadOfAccount> query,
     string? sortBy,
     bool sortDescending)
        {
            return sortBy?.ToLower() switch
            {
                "majoracccode" => sortDescending
                    ? query.OrderByDescending(h => h.MajorAccCode)
                    : query.OrderBy(h => h.MajorAccCode),

                "minoracccode" => sortDescending
                    ? query.OrderByDescending(h => h.MinorAccCode)
                    : query.OrderBy(h => h.MinorAccCode),

                "detailacccode" => sortDescending
                    ? query.OrderByDescending(h => h.DetailAccCode)
                    : query.OrderBy(h => h.DetailAccCode),

                "objectacccode" => sortDescending
                    ? query.OrderByDescending(h => h.ObjectAccCode)
                    : query.OrderBy(h => h.ObjectAccCode),

                "headdesc" => sortDescending
                    ? query.OrderByDescending(h => h.HeadDesc)
                    : query.OrderBy(h => h.HeadDesc),

                "modificationdate" => sortDescending
                    ? query.OrderByDescending(h => h.ModificationDate)
                    : query.OrderBy(h => h.ModificationDate),

                _ => query
                    .OrderBy(h => h.MajorAccCode)
                    .ThenBy(h => h.MinorAccCode)
                    .ThenBy(h => h.DetailAccCode)
                    .ThenBy(h => h.ObjectAccCode)
            };
        }


    }
}
