using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Budgets.Delete
{
    public class DeleteBudgetEntryHandler : IRequestHandler<DeleteBudgetEntryCommand, Response>
    {
        private readonly BudgetContext _context;

        public DeleteBudgetEntryHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(DeleteBudgetEntryCommand request, CancellationToken ct)
        {
            var budgetEntry = await _context.BudgetEntries
                .FirstOrDefaultAsync(b => b.Id == request.Id, ct);

            if (budgetEntry == null)
                return new Response
                {
                    IsSucced = false,
                    Message = $"Budget entry with ID '{request.Id}' not found"
                };

            _context.BudgetEntries.Remove(budgetEntry);

            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Cannot delete budget entry. It has associated records."
                };
            }

            return new Response
            {
                IsSucced = true,
                Message = "Budget entry deleted successfully"
            };
        }
    }
}
