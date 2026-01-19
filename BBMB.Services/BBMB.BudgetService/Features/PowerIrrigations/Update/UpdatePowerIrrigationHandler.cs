using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.PowerIrrigations.Update
{
    public class UpdatePowerIrrigationHandler : IRequestHandler<UpdatePowerIrrigationCommand, Response>
    {
        private readonly BudgetContext _context;

        public UpdatePowerIrrigationHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(UpdatePowerIrrigationCommand request, CancellationToken ct)
        {
            var powerIrrigation = await _context.PowerIrrigations
                .FirstOrDefaultAsync(p => p.IrrPwrCode == request.Code, ct);

            if (powerIrrigation == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Power/Irrigation code '{request.Code}' not found"
                };
            }

            powerIrrigation.IrrPwrDesc = request.dto.IrrPwrDesc;
            powerIrrigation.UserCode = request.dto.UserCode;
            powerIrrigation.ModificationDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Power/Irrigation code updated successfully"
            };
        }
    }
}
