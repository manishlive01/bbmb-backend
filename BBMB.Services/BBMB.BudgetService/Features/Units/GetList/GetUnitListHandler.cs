using BBMB.BudgetService.Features.Units.Entities;
using BBMB.BudgetService.Features.Units.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Units.GetList
{
    public class GetUnitListHandler : IRequestHandler<GetUnitListQuery, PagedResponse<GetUnitListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetUnitListHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<GetUnitListResponseDto>> Handle(GetUnitListQuery request, CancellationToken ct)
        {
            var query = _context.Units
                .Include(u => u.Project)
                .Include(u => u.PowerIrrigation)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.UnitCode))
                query = query.Where(u => u.UnitCode.Contains(request.UnitCode));

            if (!string.IsNullOrEmpty(request.UnitDesc))
                query = query.Where(u => u.UnitDesc.Contains(request.UnitDesc));

            if (!string.IsNullOrEmpty(request.ProjCode))
                query = query.Where(u => u.ProjCode == request.ProjCode);

            if (!string.IsNullOrEmpty(request.IrrPwrCode))
                query = query.Where(u => u.IrrPwrCode == request.IrrPwrCode);

            if (!string.IsNullOrEmpty(request.UserCode))
                query = query.Where(u => u.UserCode == request.UserCode);

            if (request.ModificationDateFrom.HasValue)
                query = query.Where(u => u.ModificationDate >= request.ModificationDateFrom.Value);

            if (request.ModificationDateTo.HasValue)
                query = query.Where(u => u.ModificationDate <= request.ModificationDateTo.Value);

            // Get total count
            var totalCount = await query.CountAsync(ct);

            // Apply sorting
            query = ApplySorting(query, request.SortBy, request.SortDescending);

            // Apply pagination
            var pageSize = Math.Min(request.PageSize, 100);
            var items = await query
                .Skip((request.PageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new GetUnitListResponseDto
                {
                    UnitCode = u.UnitCode,
                    UnitDesc = u.UnitDesc,
                    ProjCode = u.ProjCode,
                    ProjectDescription = u.Project != null ? u.Project.ProjDesc : string.Empty,
                    IrrPwrCode = u.IrrPwrCode,
                    PowerIrrigationDescription = u.PowerIrrigation != null ? u.PowerIrrigation.IrrPwrDesc : string.Empty,
                    ModificationDate = u.ModificationDate,
                    UserCode = u.UserCode
                })
                .ToListAsync(ct);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResponse<GetUnitListResponseDto>
            {
                Message = "Units retrieved successfully",
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

        private IQueryable<UnitMaster> ApplySorting(IQueryable<UnitMaster> query, string? sortBy, bool sortDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(u => u.UnitCode);

            return sortBy.ToLower() switch
            {
                "unitcode" => sortDescending ? query.OrderByDescending(u => u.UnitCode) : query.OrderBy(u => u.UnitCode),
                "unitdesc" => sortDescending ? query.OrderByDescending(u => u.UnitDesc) : query.OrderBy(u => u.UnitDesc),
                "projcode" => sortDescending ? query.OrderByDescending(u => u.ProjCode) : query.OrderBy(u => u.ProjCode),
                "irrpwrcode" => sortDescending ? query.OrderByDescending(u => u.IrrPwrCode) : query.OrderBy(u => u.IrrPwrCode),
                "modificationdate" => sortDescending ? query.OrderByDescending(u => u.ModificationDate) : query.OrderBy(u => u.ModificationDate),
                "usercode" => sortDescending ? query.OrderByDescending(u => u.UserCode) : query.OrderBy(u => u.UserCode),
                _ => query.OrderBy(u => u.UnitCode)
            };
        }
    }
}
