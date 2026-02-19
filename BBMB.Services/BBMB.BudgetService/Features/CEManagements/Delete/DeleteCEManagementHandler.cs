using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.CEManagements.Delete
{
    public class DeleteCEManagementHandler
        : IRequestHandler<DeleteCEManagementCommand, Response>
    {
        private readonly BudgetContext _context;

        public DeleteCEManagementHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(DeleteCEManagementCommand request, CancellationToken ct)
        {
            var ce = await _context.CEManagements
                .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

            if (ce == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"CE Management record with ID '{request.Id}' not found"
                };
            }

            _context.CEManagements.Remove(ce);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "CE Management record deleted successfully"
            };
        }
    }
}
