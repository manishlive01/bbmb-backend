using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Works.Delete
{
    public class DeleteWorkHandler : IRequestHandler<DeleteWorkCommand, Response>
    {
        private readonly BudgetContext _context;

        public DeleteWorkHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(DeleteWorkCommand request, CancellationToken ct)
        {
            var work = await _context.Works
                .FirstOrDefaultAsync(w => w.WorkCode == request.Code, ct);

            if (work == null)
                return new Response
                {
                    IsSucced = false,
                    Message = $"Work with code '{request.Code}' not found"
                };

            // Check for dependencies (budget entries)
            var hasBudgetEntries = await _context.BudgetEntries
                .AnyAsync(b => b.WorkCode == request.Code, ct);

            if (hasBudgetEntries)
                return new Response
                {
                    IsSucced = false,
                    Message = "Cannot delete work. It has associated budget entries."
                };

            _context.Works.Remove(work);

            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Cannot delete work. It has associated records."
                };
            }

            return new Response
            {
                IsSucced = true,
                Message = "Work deleted successfully"
            };
        }
    }
}
