using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Units.Delete
{
    public class DeleteUnitHandler : IRequestHandler<DeleteUnitCommand, Response>
    {
        private readonly BudgetContext _context;

        public DeleteUnitHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(DeleteUnitCommand request, CancellationToken ct)
        {
            var unit = await _context.Units
                .FirstOrDefaultAsync(u => u.UnitCode == request.Code, ct);

            if (unit == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Unit with code '{request.Code}' not found"
                };
            }

            _context.Units.Remove(unit);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Unit deleted successfully"
            };
        }
    }
}
