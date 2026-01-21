using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Designations.Delete
{
    public class DeleteDesignationHandler
        : IRequestHandler<DeleteDesignationCommand, Response>
    {
        private readonly BudgetContext _context;

        public DeleteDesignationHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(
            DeleteDesignationCommand request,
            CancellationToken ct)
        {
            // 1️⃣ Fetch WITHOUT tracking
            var designation = await _context.Designations
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    d => d.DesignationCode == request.dto.Id,
                    ct);

            if (designation == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Designation with code '{request.dto.Id}' not found"
                };
            }

            // 2️⃣ Check associated works (NO tracking)
            var hasAssociatedWorks = await _context.Works
                .AsNoTracking()
                .AnyAsync(
                    w => w.DesignationCode == request.dto.Id,
                    ct);

            if (hasAssociatedWorks)
            {
                var workCodes = await _context.Works
                    .AsNoTracking()
                    .Where(w => w.DesignationCode == request.dto.Id)
                    .Select(w => w.WorkCode)
                    .ToListAsync(ct);

                return new Response
                {
                    IsSucced = false,
                    Message =
                        $"Cannot delete designation. Associated works: {string.Join(", ", workCodes)}"
                };
            }

            // 3️⃣ Attach + HARD DELETE
            _context.Designations.Attach(designation);
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
