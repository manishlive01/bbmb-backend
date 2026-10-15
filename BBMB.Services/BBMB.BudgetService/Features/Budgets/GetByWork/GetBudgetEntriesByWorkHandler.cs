using BBMB.BudgetService.Features.Budgets.Entities;
using BBMB.BudgetService.Features.Budgets.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Budgets.GetByWork
{
    public class GetBudgetEntriesByWorkHandler : IRequestHandler<GetBudgetEntriesByWorkQuery, PagedResponse<GetBudgetEntryListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetBudgetEntriesByWorkHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<GetBudgetEntryListResponseDto>> Handle(GetBudgetEntriesByWorkQuery request, CancellationToken ct)
        {
            // P4 compile fix (design §4.6): work_code is now string?; this by-work route is re-keyed to workId
            // in P4-03, so the comparison is kept string-wise until then.
            var workCodeText = request.WorkCode.ToString();
            var query = _context.BudgetEntries
                .Include(b => b.Division)
                .Include(b => b.Work)
                .Where(b => b.Work.WorkCode == workCodeText);

            var totalCount = await query.CountAsync(ct);

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
                    //DivisionCode = b.DivisionCode,
                    DivisionDesc = b.Division?.DivisionDesc,
                    CalendarYear = b.CalendarYear,
                    AccountCode = $"{b.MajorAccCode}-{b.SubMajorAccCode}-{b.MinorAccCode}-{b.SubMinorAccCode}-{b.DetailAccCode}-{b.ObjectAccCode}",
                    AccountDescription = accountDescription,
                    //WorkCode = b.WorkCode,
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
