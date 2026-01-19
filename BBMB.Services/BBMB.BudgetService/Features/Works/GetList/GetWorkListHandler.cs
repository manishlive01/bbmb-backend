using BBMB.BudgetService.Features.Works.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Works.GetList
{
    public class GetWorkListHandler : IRequestHandler<GetWorkListQuery, PagedResponse<GetWorkListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetWorkListHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<GetWorkListResponseDto>> Handle(GetWorkListQuery request, CancellationToken ct)
        {
            var query = _context.Works
                .Include(w => w.Division)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.WorkCode))
                query = query.Where(w => w.WorkCode.Contains(request.WorkCode));

            if (!string.IsNullOrEmpty(request.WorkDesc))
                query = query.Where(w => w.WorkDesc.Contains(request.WorkDesc));

            if (request.DivisionCode.HasValue)
                query = query.Where(w => w.DivisionCode == request.DivisionCode.Value);

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(w => w.Status == request.Status);

            if (!string.IsNullOrEmpty(request.Nature))
                query = query.Where(w => w.Nature == request.Nature);

            // Get total count
            var totalCount = await query.CountAsync(ct);

            // Apply sorting
            query = ApplySorting(query, request.SortBy, request.SortDescending);

            // Apply pagination
            var pageSize = Math.Min(request.PageSize, 100);
            var items = await query
                .Skip((request.PageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(w => new GetWorkListResponseDto
                {
                    WorkCode = w.WorkCode,
                    WorkDesc = w.WorkDesc,
                    DivisionCode = w.DivisionCode,
                    DivisionDesc = w.Division != null ? w.Division.DivisionDesc : null,
                    AccountCode = $"{w.MajorAccCode}-{w.SubMajorAccCode}-{w.MinorAccCode}-{w.SubMinorAccCode}-{w.DetailAccCode}-{w.ObjectAccCode}",
                    Status = w.Status,
                    Nature = w.Nature,
                    UserCode = w.UserCode
                })
                .ToListAsync(ct);

            return new PagedResponse<GetWorkListResponseDto>
            {
                Message = "Works retrieved successfully",
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

        private IQueryable<BBMB.BudgetService.Features.Works.Entities.Work> ApplySorting(
            IQueryable<BBMB.BudgetService.Features.Works.Entities.Work> query, string? sortBy, bool sortDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(w => w.WorkCode);

            return sortBy.ToLower() switch
            {
                "workcode" => sortDescending ? query.OrderByDescending(w => w.WorkCode) : query.OrderBy(w => w.WorkCode),
                "workdesc" => sortDescending ? query.OrderByDescending(w => w.WorkDesc) : query.OrderBy(w => w.WorkDesc),
                "divisioncode" => sortDescending ? query.OrderByDescending(w => w.DivisionCode) : query.OrderBy(w => w.DivisionCode),
                _ => query.OrderBy(w => w.WorkCode)
            };
        }
    }
}
