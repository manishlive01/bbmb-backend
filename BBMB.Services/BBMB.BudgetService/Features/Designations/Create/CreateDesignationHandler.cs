using BBMB.BudgetService.Features.Designations.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Designations.Create
{
    public class CreateDesignationHandler : IRequestHandler<CreateDesignationCommand, Response>
    {
        private readonly BudgetContext _context;

        public CreateDesignationHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(CreateDesignationCommand request, CancellationToken ct)
        {
            // Check for duplicate
            var exists = await _context.Designations
                .AnyAsync(d => d.DesignationCode == request.dto.DesignationCode, ct);

            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Designation with code '{request.dto.DesignationCode}' already exists"
                };
            }

            // Map DTO to Entity
            var designation = request.dto.Adapt<Designation>();
            designation.ModificationDate = DateTime.UtcNow;

            // Persist
            _context.Designations.Add(designation);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Designation created successfully"
            };
        }
    }
}
