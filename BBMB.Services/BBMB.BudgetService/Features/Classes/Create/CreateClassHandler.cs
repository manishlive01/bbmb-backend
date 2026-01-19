using BBMB.BudgetService.Features.Classes.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Classes.Create
{
    public class CreateClassHandler : IRequestHandler<CreateClassCommand, Response>
    {
        private readonly BudgetContext _context;

        public CreateClassHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(CreateClassCommand request, CancellationToken ct)
        {
            // Check for duplicate
            var exists = await _context.Classes
                .AnyAsync(c => c.ClassCode == request.dto.ClassCode, ct);

            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Class with code '{request.dto.ClassCode}' already exists"
                };
            }

            // Map DTO to Entity
            var classEntity = request.dto.Adapt<Class>();
            classEntity.ModificationDate = DateTime.UtcNow;

            // Persist
            _context.Classes.Add(classEntity);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Class created successfully"
            };
        }
    }
}
