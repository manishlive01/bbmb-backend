using BBMB.BudgetService.Features.PowerIrrigations.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.PowerIrrigations.Create
{
    public class CreatePowerIrrigationHandler : IRequestHandler<CreatePowerIrrigationCommand, Response>
    {
        private readonly BudgetContext _context;

        public CreatePowerIrrigationHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(CreatePowerIrrigationCommand request, CancellationToken ct)
        {
            var exists = await _context.PowerIrrigations
                .AnyAsync(p => p.IrrPwrCode == request.dto.IrrPwrCode, ct);

            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Power/Irrigation code '{request.dto.IrrPwrCode}' already exists"
                };
            }

            var powerIrrigation = request.dto.Adapt<PowerIrrigation>();
            powerIrrigation.ModificationDate = DateTime.UtcNow;

            _context.PowerIrrigations.Add(powerIrrigation);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Power/Irrigation code created successfully"
            };
        }
    }
}
