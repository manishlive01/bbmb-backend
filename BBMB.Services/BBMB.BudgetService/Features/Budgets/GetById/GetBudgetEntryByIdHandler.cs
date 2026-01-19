using BBMB.BudgetService.Features.Budgets.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Budgets.GetById
{
    public class GetBudgetEntryByIdHandler : IRequestHandler<GetBudgetEntryByIdQuery, GetBudgetEntryByIdResponseDto>
    {
        private readonly BudgetContext _context;

        public GetBudgetEntryByIdHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<GetBudgetEntryByIdResponseDto> Handle(GetBudgetEntryByIdQuery request, CancellationToken ct)
        {
            var budgetEntry = await _context.BudgetEntries
                .Include(b => b.Division)
                .Include(b => b.Work)
                .FirstOrDefaultAsync(b => b.Id == request.Id, ct);

            if (budgetEntry == null)
            {
                throw new KeyNotFoundException($"Budget entry with ID '{request.Id}' not found");
            }

            var response = budgetEntry.Adapt<GetBudgetEntryByIdResponseDto>();
            response.DivisionDesc = budgetEntry.Division?.DivisionDesc;
            response.WorkDesc = budgetEntry.Work?.WorkDesc;
            response.AccountCode = $"{budgetEntry.MajorAccCode}-{budgetEntry.SubMajorAccCode}-{budgetEntry.MinorAccCode}-{budgetEntry.SubMinorAccCode}-{budgetEntry.DetailAccCode}-{budgetEntry.ObjectAccCode}";

            // Get account description from HeadOfAccount if exists
            var accountDescription = await GetAccountDescription(budgetEntry, ct);
            response.AccountDescription = accountDescription;

            return response;
        }

        private async Task<string?> GetAccountDescription(Entities.BudgetEntry budgetEntry, CancellationToken ct)
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
