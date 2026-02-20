using BBMB.BudgetService.Features.CRManagements.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.CRManagements.Delete
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
                .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

            if (cr == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"CR Management record with ID '{request.Id}' not found"
                };
            }

            // 🔎 Optional: Check if dependent records exist
            // Example: If CR is referenced somewhere else
            // var isUsed = await _context.SomeOtherTable
            //     .AnyAsync(x => x.CRId == request.Id, ct);
            // if (isUsed)
            // {
            //     return new Response
            //     {
            //         IsSucced = false,
            //         Message = "CR cannot be deleted because it is in use"
            //     };
            // }

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
