using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRManagements.Delete
{
    public class DeleteCRManagementHandler
        : IRequestHandler<DeleteCRManagementCommand, Response>
    {
        private readonly BudgetContext _context;

        public DeleteCRManagementHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(DeleteCRManagementCommand request, CancellationToken ct)
        {
            // 🔎 Check if CR exists
            var cr = await _context.CRManagements
                .FirstOrDefaultAsync(x => x.CRCode == request.crCode, ct);

            if (cr == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"CR Management record with Code '{request.crCode}' not found"
                };
            }

            // Example: If CR is referenced somewhere else
            var isUsed = await _context.Divisions
                .AnyAsync(x => x.CRCode == request.crCode, ct);
            if (isUsed)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "CR cannot be deleted because it is use in Division."
                };
            }

            // ✅ Hard Delete
            _context.CRManagements.Remove(cr);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "CR Management record deleted successfully"
            };
        }
    }
}
