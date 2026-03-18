using BBMB.BudgetService.Features.Offices.OfficeManagements.Entities;
using BBMB.BudgetService.Features.Offices.OfficeManagements.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Offices.OfficeManagements.GetList
{
    public class GetOfficeManagementListHandler
        : IRequestHandler<GetOfficeManagementListQuery, PagedResponse<GetOfficeManagementListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetOfficeManagementListHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<GetOfficeManagementListResponseDto>> Handle(
            GetOfficeManagementListQuery request,
            CancellationToken ct)
        {
            var query = _context.OfficeManagements.AsQueryable();

            // 🔍 Filters
            if (!string.IsNullOrEmpty(request.OfficeCode))
                query = query.Where(x => x.OfficeCode == request.OfficeCode);

            if (!string.IsNullOrEmpty(request.OfficeDesc))
                query = query.Where(x => x.OfficeDesc.Contains(request.OfficeDesc));

            if (request.CECode.HasValue)
                query = query.Where(x => x.CECode == request.CECode);

            if (request.CRCode.HasValue)
                query = query.Where(x => x.CRCode == request.CRCode);

            if (request.DivisionCode.HasValue)
                query = query.Where(x => x.DivisionCode == request.DivisionCode);

            if (!string.IsNullOrEmpty(request.UserCode))
                query = query.Where(x => x.UserCode == request.UserCode);

            if (request.ModificationDateFrom.HasValue)
                query = query.Where(x => x.ModificationDate >= request.ModificationDateFrom);

            if (request.ModificationDateTo.HasValue)
                query = query.Where(x => x.ModificationDate <= request.ModificationDateTo);

            // 📊 Total Count
            var totalCount = await query.CountAsync(ct);

            // 🔽 Sorting
            query = ApplySorting(query, request.SortBy, request.SortDescending);

            var pageSize = Math.Min(request.PageSize, 100);

            var data = await query
                .Skip((request.PageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            // 🔥 Fetch related master data (OPTIMIZED)
            var ceCodes = data.Select(x => x.CECode).Distinct().ToList();
            var crCodes = data.Where(x => x.CRCode.HasValue).Select(x => x.CRCode!.Value).Distinct().ToList();
            var divCodes = data.Where(x => x.DivisionCode.HasValue).Select(x => x.DivisionCode!.Value).Distinct().ToList();

            var ces = await _context.CEManagements
                .Where(x => ceCodes.Contains(x.CECode))
                .ToDictionaryAsync(x => x.CECode, ct);

            var crs = await _context.CRManagements
                .Where(x => crCodes.Contains(x.CRCode))
                .ToDictionaryAsync(x => x.CRCode, ct);

            var divs = await _context.Divisions
                .Where(x => divCodes.Contains(x.DivisionCode))
                .ToDictionaryAsync(x => x.DivisionCode, ct);

            // 🔁 Mapping
            var items = data.Select(ec => new GetOfficeManagementListResponseDto
            {
                OfficeCode = ec.OfficeCode,
                OfficeDesc = ec.OfficeDesc,

                CECode = ec.CECode,
                CEName = ces.ContainsKey(ec.CECode)
                    ? ces[ec.CECode].Name
                    : "",

                CRCode = ec.CRCode,
                CRName = ec.CRCode.HasValue && crs.ContainsKey(ec.CRCode.Value)
                    ? crs[ec.CRCode.Value].Name
                    : "",

                DivisionCode = ec.DivisionCode,
                DivName = ec.DivisionCode.HasValue && divs.ContainsKey(ec.DivisionCode.Value)
                    ? divs[ec.DivisionCode.Value].DivisionDesc
                    : "",

                ModificationDate = ec.ModificationDate,
                UserCode = ec.UserCode
            }).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResponse<GetOfficeManagementListResponseDto>
            {
                Message = "Office management list retrieved successfully",
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

        private IQueryable<OfficeManagement> ApplySorting(
            IQueryable<OfficeManagement> query,
            string? sortBy,
            bool sortDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(x => x.OfficeCode);

            return sortBy.ToLower() switch
            {
                "officecode" => sortDescending ? query.OrderByDescending(x => x.OfficeCode) : query.OrderBy(x => x.OfficeCode),
                "officedesc" => sortDescending ? query.OrderByDescending(x => x.OfficeDesc) : query.OrderBy(x => x.OfficeDesc),
                "modificationdate" => sortDescending ? query.OrderByDescending(x => x.ModificationDate) : query.OrderBy(x => x.ModificationDate),
                _ => query.OrderBy(x => x.OfficeCode)
            };
        }
    }
}