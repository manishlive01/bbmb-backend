using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.PowerIrrigations.Delete
{
    public class DeletePowerIrrigationHandler : IRequestHandler<DeletePowerIrrigationCommand, Response>
    {
        private readonly BudgetContext _context;

        public DeletePowerIrrigationHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(DeletePowerIrrigationCommand request, CancellationToken ct)
        {
            var powerIrrigation = await _context.PowerIrrigations
                .Include(p => p.Units)
                .FirstOrDefaultAsync(p => p.IrrPwrCode == request.Code, ct);

            if (powerIrrigation == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Power/Irrigation code '{request.Code}' not found"
                };
            }

            // Check for dependencies
            if (powerIrrigation.Units.Any())
            {
                var unitCodes = string.Join(", ", powerIrrigation.Units.Select(u => u.UnitCode));
                return new Response
                {
                    IsSucced = false,
                    Message = $"Cannot delete Power/Irrigation code. Associated units: {unitCodes}"
                };
            }

            _context.PowerIrrigations.Remove(powerIrrigation);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Power/Irrigation code deleted successfully"
            };
        }
    }
}
