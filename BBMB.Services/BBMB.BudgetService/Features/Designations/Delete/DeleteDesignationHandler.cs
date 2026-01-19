using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Designations.Delete
{
    public class DeleteDesignationHandler : IRequestHandler<DeleteDesignationCommand, Response>
    {
        private readonly BudgetContext _context;

        public DeleteDesignationHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(DeleteDesignationCommand request, CancellationToken ct)
        {
            var designation = await _context.Designations
                .FirstOrDefaultAsync(d => d.DesignationCode == request.Code, ct);

            if (designation == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Designation with code '{request.Code}' not found"
                };
            }

            // Check for associated works
            var associatedWorks = await _context.Works
                .Where(w => w.DesignationCode == request.Code)
                .Select(w => w.WorkCode)
                .ToListAsync(ct);

            if (associatedWorks.Any())
            {
                var workCodes = string.Join(", ", associatedWorks);
                return new Response
                {
                    IsSucced = false,
                    Message = $"Cannot delete designation. Associated works: {workCodes}"
                };
            }

            _context.Designations.Remove(designation);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Designation deleted successfully"
            };
        }
    }
}
