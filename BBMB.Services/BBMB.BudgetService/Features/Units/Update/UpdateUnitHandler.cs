using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Units.Update
{
    public class UpdateUnitHandler : IRequestHandler<UpdateUnitCommand, Response>
    {
        private readonly BudgetContext _context;

        public UpdateUnitHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(UpdateUnitCommand request, CancellationToken ct)
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

            // Validate foreign keys
            var projectExists = await _context.Projects
                .AnyAsync(p => p.ProjCode == request.dto.ProjCode, ct);

            if (!projectExists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Project with code '{request.dto.ProjCode}' does not exist"
                };
            }

            var powerIrrigationExists = await _context.PowerIrrigations
                .AnyAsync(pi => pi.IrrPwrCode == request.dto.IrrPwrCode, ct);

            if (!powerIrrigationExists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Power/Irrigation with code '{request.dto.IrrPwrCode}' does not exist"
                };
            }

            unit.UnitDesc = request.dto.UnitDesc;
            unit.ProjCode = request.dto.ProjCode;
            unit.IrrPwrCode = request.dto.IrrPwrCode;
            unit.UserCode = request.dto.UserCode;
            unit.ModificationDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Unit updated successfully"
            };
        }
    }
}
