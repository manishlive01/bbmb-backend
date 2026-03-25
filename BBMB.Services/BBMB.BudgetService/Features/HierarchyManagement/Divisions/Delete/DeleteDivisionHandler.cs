using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.HierarchyManagement.Divisions.Delete
{
    public class DeleteDivisionHandler : IRequestHandler<DeleteDivisionCommand, Response>
    {
        private readonly BudgetContext _context;

        public DeleteDivisionHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(DeleteDivisionCommand request, CancellationToken ct)
        {
            var division = await _context.Divisions
                .FirstOrDefaultAsync(d => d.Id == request.divId, ct);

            if (division == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Division with Id '{request.divId}' not found"
                };
            }

            _context.Divisions.Remove(division);

            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Cannot delete division. It has associated records."
                };
            }

            return new Response
            {
                IsSucced = true,
                Message = "Division deleted successfully"
            };
        }
    }
}
