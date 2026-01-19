using BBMB.BudgetService.Features.PowerIrrigations.Entities;
using BBMB.BudgetService.Features.PowerIrrigations.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.PowerIrrigations.GetList
{
    public class GetPowerIrrigationListHandler : IRequestHandler<GetPowerIrrigationListQuery, PagedResponse<GetPowerIrrigationListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetPowerIrrigationListHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<GetPowerIrrigationListResponseDto>> Handle(GetPowerIrrigationListQuery request, CancellationToken ct)
        {
            var query = _context.PowerIrrigations.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.IrrPwrCode))
                query = query.Where(p => p.IrrPwrCode.Contains(request.IrrPwrCode));

            if (!string.IsNullOrEmpty(request.IrrPwrDesc))
                query = query.Where(p => p.IrrPwrDesc.Contains(request.IrrPwrDesc));

            if (!string.IsNullOrEmpty(request.UserCode))
                query = query.Where(p => p.UserCode == request.UserCode);

            if (request.ModificationDateFrom.HasValue)
                query = query.Where(p => p.ModificationDate >= request.ModificationDateFrom.Value);

            if (request.ModificationDateTo.HasValue)
                query = query.Where(p => p.ModificationDate <= request.ModificationDateTo.Value);

            // Get total count
            var totalCount = await query.CountAsync(ct);

            // Apply sorting
            query = ApplySorting(query, request.SortBy, request.SortDescending);

            // Apply pagination
            var pageSize = Math.Min(request.PageSize, 100);
            var items = await query
                .Skip((request.PageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new GetPowerIrrigationListResponseDto
                {
                    IrrPwrCode = p.IrrPwrCode,
                    IrrPwrDesc = p.IrrPwrDesc,
                    ModificationDate = p.ModificationDate,
                    UserCode = p.UserCode
                })
                .ToListAsync(ct);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResponse<GetPowerIrrigationListResponseDto>
            {
                Message = "Power/Irrigation codes retrieved successfully",
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

        private IQueryable<PowerIrrigation> ApplySorting(IQueryable<PowerIrrigation> query, string? sortBy, bool sortDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(p => p.IrrPwrCode);

            return sortBy.ToLower() switch
            {
                "irrpwrcode" => sortDescending ? query.OrderByDescending(p => p.IrrPwrCode) : query.OrderBy(p => p.IrrPwrCode),
                "irrpwrdesc" => sortDescending ? query.OrderByDescending(p => p.IrrPwrDesc) : query.OrderBy(p => p.IrrPwrDesc),
                "modificationdate" => sortDescending ? query.OrderByDescending(p => p.ModificationDate) : query.OrderBy(p => p.ModificationDate),
                "usercode" => sortDescending ? query.OrderByDescending(p => p.UserCode) : query.OrderBy(p => p.UserCode),
                _ => query.OrderBy(p => p.IrrPwrCode)
            };
        }
    }
}
