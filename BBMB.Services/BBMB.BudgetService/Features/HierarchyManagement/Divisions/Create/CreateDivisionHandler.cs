using BBMB.BudgetService.Features.HierarchyManagement.Divisions.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.HierarchyManagement.Divisions.Create
{
    public class CreateDivisionHandler : IRequestHandler<CreateDivisionCommand, Response>
    {
        private readonly BudgetContext _context;

        public CreateDivisionHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(CreateDivisionCommand request, CancellationToken ct)
        {
            // Check for duplicate
            var exists = await _context.Divisions
                .AnyAsync(d => d.DivisionCode == request.dto.DivisionCode, ct);

            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Division with code '{request.dto.DivisionCode}' already exists"
                };
            }

            // Map DTO to Entity
            var division = request.dto.Adapt<Division>();
            division.ModificationDate = DateTime.UtcNow;

            // Persist
            _context.Divisions.Add(division);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Division created successfully"
            };
        }
    }
}
