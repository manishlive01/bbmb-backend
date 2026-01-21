using BBMB.BudgetService.Features.Offices.RemittanceOffices.Entities;
using BBMB.BudgetService.Features.Offices.RemittanceOffices.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Offices.RemittanceOffices.GetList
{
    public class GetRemittanceListHandler:IRequestHandler<GetRemittanceListQuery, PagedResponse<GetRemittanceListResponseDto>>
    {
        private readonly BudgetContext _context;

    public GetRemittanceListHandler(BudgetContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<GetRemittanceListResponseDto>> Handle(GetRemittanceListQuery request, CancellationToken ct)
    {
        var query = _context.RemittanceOffices.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.ROCode))
            query = query.Where(p => p.RoCode.Contains(request.ROCode));

        if (!string.IsNullOrEmpty(request.RODesc))
            query = query.Where(p => p.RoDesc.Contains(request.RODesc));

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
            .Select(p => new GetRemittanceListResponseDto
            {
                ROCode = p.RoCode,
                RODesc = p.RoDesc,
                ModificationDate = p.ModificationDate,
                UserCode = p.UserCode
            })
            .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResponse<GetRemittanceListResponseDto>
        {
            Message = "Pools retrieved successfully",
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

    private IQueryable<RemittanceOffice> ApplySorting(IQueryable<RemittanceOffice> query, string? sortBy, bool sortDescending)
    {
        if (string.IsNullOrEmpty(sortBy))
            return query.OrderBy(p => p.RoCode);

        return sortBy.ToLower() switch
        {
            "rocode" => sortDescending ? query.OrderByDescending(p => p.RoCode) : query.OrderBy(p => p.RoCode),
            "rodesc" => sortDescending ? query.OrderByDescending(p => p.RoDesc) : query.OrderBy(p => p.RoDesc),
            "modificationdate" => sortDescending ? query.OrderByDescending(p => p.ModificationDate) : query.OrderBy(p => p.ModificationDate),
            "usercode" => sortDescending ? query.OrderByDescending(p => p.UserCode) : query.OrderBy(p => p.UserCode),
            _ => query.OrderBy(p => p.RoCode)
        };
    }
}
}
