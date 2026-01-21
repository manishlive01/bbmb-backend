using BBMB.BudgetService.Features.Offices.AccountOffices.Entities;
using BBMB.BudgetService.Features.Offices.AccountOffices.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Offices.AccountOffices.GetList
{
    public class GetAccountOfficeListHandler
        : IRequestHandler<GetAccountOfficeListQuery, PagedResponse<GetAccountOfficeListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetAccountOfficeListHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<GetAccountOfficeListResponseDto>> Handle(
            GetAccountOfficeListQuery request,
            CancellationToken ct)
        {
            var query = _context.AccountOffices.AsQueryable();

            // ===== Apply filters =====
            if (!string.IsNullOrEmpty(request.AccOffice))
                query = query.Where(a => a.AccOffice.Contains(request.AccOffice));

            if (!string.IsNullOrEmpty(request.AccOfficeDesc))
                query = query.Where(a => a.AccOfficeDesc.Contains(request.AccOfficeDesc));

            if (!string.IsNullOrEmpty(request.UserCode))
                query = query.Where(a => a.UserCode == request.UserCode);

            if (request.ModificationDateFrom.HasValue)
                query = query.Where(a => a.ModificationDate >= request.ModificationDateFrom.Value);

            if (request.ModificationDateTo.HasValue)
                query = query.Where(a => a.ModificationDate <= request.ModificationDateTo.Value);

            // ===== Total count =====
            var totalCount = await query.CountAsync(ct);

            // ===== Sorting =====
            query = ApplySorting(query, request.SortBy, request.SortDescending);

            // ===== Pagination =====
            var pageSize = Math.Min(request.PageSize, 100);
            var items = await query
                .Skip((request.PageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new GetAccountOfficeListResponseDto
                {
                    AccOffice = a.AccOffice,
                    AccOfficeDesc = a.AccOfficeDesc,
                    ModificationDate = a.ModificationDate,
                    UserCode = a.UserCode
                })
                .ToListAsync(ct);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResponse<GetAccountOfficeListResponseDto>
            {
                Message = "Account offices retrieved successfully",
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

        private IQueryable<AccountOffice> ApplySorting(
            IQueryable<AccountOffice> query,
            string? sortBy,
            bool sortDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(a => a.AccOffice);

            return sortBy.ToLower() switch
            {
                "accoffice" =>
                    sortDescending ? query.OrderByDescending(a => a.AccOffice) : query.OrderBy(a => a.AccOffice),

                "accofficedesc" =>
                    sortDescending ? query.OrderByDescending(a => a.AccOfficeDesc) : query.OrderBy(a => a.AccOfficeDesc),

                "modificationdate" =>
                    sortDescending ? query.OrderByDescending(a => a.ModificationDate) : query.OrderBy(a => a.ModificationDate),

                "usercode" =>
                    sortDescending ? query.OrderByDescending(a => a.UserCode) : query.OrderBy(a => a.UserCode),

                _ => query.OrderBy(a => a.AccOffice)
            };
        }
    }
}
