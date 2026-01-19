using BBMB.BudgetService.Features.Budgets.Entities;
using BBMB.BudgetService.Features.Budgets.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Budgets.GetList
{
    public class GetBudgetEntryListHandler : IRequestHandler<GetBudgetEntryListQuery, PagedResponse<GetBudgetEntryListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetBudgetEntryListHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<GetBudgetEntryListResponseDto>> Handle(GetBudgetEntryListQuery request, CancellationToken ct)
        {
            var query = _context.BudgetEntries
                .Include(b => b.Division)
                .Include(b => b.Work)
                .AsQueryable();

            // Apply filters
            if (request.DivisionCode.HasValue)
                query = query.Where(b => b.DivisionCode == request.DivisionCode.Value);

            if (request.CalendarYear.HasValue)
                query = query.Where(b => b.CalendarYear == request.CalendarYear.Value);

            if (request.BudgetType.HasValue)
                query = query.Where(b => b.BudgetType == request.BudgetType.Value);

            if (!string.IsNullOrEmpty(request.WorkCode))
                query = query.Where(b => b.WorkCode == request.WorkCode);

            // Get total count
            var totalCount = await query.CountAsync(ct);

            // Apply sorting
            query = ApplySorting(query, request.SortBy, request.SortDescending);

            // Apply pagination
            var pageSize = Math.Min(request.PageSize, 100);
            var budgetEntries = await query
                .Skip((request.PageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            // Get account descriptions for all entries
            var items = new List<GetBudgetEntryListResponseDto>();
            foreach (var b in budgetEntries)
            {
                var accountDescription = await GetAccountDescription(b, ct);
                items.Add(new GetBudgetEntryListResponseDto
                {
                    Id = b.Id,
                    DivisionCode = b.DivisionCode,
                    DivisionDesc = b.Division?.DivisionDesc,
                    CalendarYear = b.CalendarYear,
                    AccountCode = $"{b.MajorAccCode}-{b.SubMajorAccCode}-{b.MinorAccCode}-{b.SubMinorAccCode}-{b.DetailAccCode}-{b.ObjectAccCode}",
                    AccountDescription = accountDescription,
                    WorkCode = b.WorkCode,
                    WorkDesc = b.Work?.WorkDesc,
                    BudgetDescription = b.BudgetDescription,
                    BudgetAmount = b.BudgetAmount,
                    BudgetType = b.BudgetType,
                    UserCode = b.UserCode
                });
            }

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResponse<GetBudgetEntryListResponseDto>
            {
                Message = "Budget entries retrieved successfully",
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

        private IQueryable<BudgetEntry> ApplySorting(IQueryable<BudgetEntry> query, string? sortBy, bool sortDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(b => b.Id);

            return sortBy.ToLower() switch
            {
                "id" => sortDescending ? query.OrderByDescending(b => b.Id) : query.OrderBy(b => b.Id),
                "calendaryear" => sortDescending ? query.OrderByDescending(b => b.CalendarYear) : query.OrderBy(b => b.CalendarYear),
                "divisioncode" => sortDescending ? query.OrderByDescending(b => b.DivisionCode) : query.OrderBy(b => b.DivisionCode),
                "budgetamount" => sortDescending ? query.OrderByDescending(b => b.BudgetAmount) : query.OrderBy(b => b.BudgetAmount),
                _ => query.OrderBy(b => b.Id)
            };
        }

        private async Task<string?> GetAccountDescription(BudgetEntry budgetEntry, CancellationToken ct)
        {
            var headOfAccount = await _context.HeadOfAccounts
                .FirstOrDefaultAsync(h =>
                    h.MajorAccCode == budgetEntry.MajorAccCode &&
                    h.SubMajorAccCode == budgetEntry.SubMajorAccCode &&
                    h.MinorAccCode == budgetEntry.MinorAccCode &&
                    h.SubMinorAccCode == budgetEntry.SubMinorAccCode &&
                    h.DetailAccCode == budgetEntry.DetailAccCode &&
                    h.ObjectAccCode == budgetEntry.ObjectAccCode, ct);

            return headOfAccount?.HeadDesc;
        }
    }
}
