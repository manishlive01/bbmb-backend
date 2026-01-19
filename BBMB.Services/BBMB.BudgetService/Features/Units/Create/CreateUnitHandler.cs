using BBMB.BudgetService.Features.Units.Create.Dtos;
using BBMB.BudgetService.Features.Units.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Units.Create
{
    public class CreateUnitHandler : IRequestHandler<CreateUnitCommand, Response>
    {
        private readonly BudgetContext _context;

        public CreateUnitHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(CreateUnitCommand request, CancellationToken ct)
        {
            // Check for duplicate unit code
            var exists = await _context.Units
                .AnyAsync(u => u.UnitCode == request.dto.UnitCode, ct);

            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Unit with code '{request.dto.UnitCode}' already exists"
                };
            }

            // Validate foreign key: Project Code
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

            // Validate foreign key: Power/Irrigation Code
            var powerIrrigationExists = await _context.PowerIrrigations
                .AnyAsync(pi => pi.IrrPwrCode == request.dto.IrrPwrCode, ct);

            if (!powerIrrigationExists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Power/Irrigation code '{request.dto.IrrPwrCode}' does not exist"
                };
            }

            // Map DTO to Entity
            var unit = request.dto.Adapt<UnitMaster>();
            unit.ModificationDate = DateTime.UtcNow;

            // Persist
            _context.Units.Add(unit);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Unit created successfully"
                };
        }
    }
}
