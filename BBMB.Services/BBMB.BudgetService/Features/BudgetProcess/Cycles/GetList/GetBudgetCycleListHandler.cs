using BBMB.BudgetService.Features.BudgetProcess.Cycles.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Cycles.GetList
{
    /// <summary>Cycles, newest first. Not office-scoped (a cycle belongs to no office).</summary>
    public sealed class GetBudgetCycleListHandler(BudgetContext context)
        : IRequestHandler<GetBudgetCycleListQuery, PagedResponse<BudgetCycleResponseDto>>
    {
        public async Task<PagedResponse<BudgetCycleResponseDto>> Handle(GetBudgetCycleListQuery request, CancellationToken ct)
        {
            var query = context.BudgetCycles.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(c => c.Status == request.Status);
            var pageNumber = Math.Max(request.PageNumber, 1);
            var pageSize = Math.Min(request.PageSize <= 0 ? 10 : request.PageSize, 100);
            var total = await query.CountAsync(ct);
            var items = await query.OrderByDescending(c => c.CurrentFinYear)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(BudgetCycleProjection.ToDto).ToListAsync(ct);
            var pages = (int)Math.Ceiling(total / (double)pageSize);
            return new PagedResponse<BudgetCycleResponseDto>
            {
                Message = "Budget cycles retrieved successfully",
                Data = items,
                Pagination = new PaginationMetadata
                {
                    TotalCount = total, PageNumber = pageNumber, PageSize = pageSize, TotalPages = pages,
                    HasPreviousPage = pageNumber > 1, HasNextPage = pageNumber < pages,
                },
            };
        }
    }
}
