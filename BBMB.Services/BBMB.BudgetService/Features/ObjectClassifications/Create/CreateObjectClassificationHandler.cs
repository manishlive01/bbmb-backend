using BBMB.BudgetService.Features.ObjectClassifications.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.ObjectClassifications.Create
{
    public class CreateObjectClassificationHandler : IRequestHandler<CreateObjectClassificationCommand, Response>
    {
        private readonly BudgetContext _context;

        public CreateObjectClassificationHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(CreateObjectClassificationCommand request, CancellationToken ct)
        {
            // Check for duplicate
            var exists = await _context.ObjectClassifications
                .AnyAsync(o => o.ObjectAccCode == request.dto.ObjectAccCode, ct);

            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Object classification with code '{request.dto.ObjectAccCode}' already exists"
                };
            }

            // Map DTO to Entity
            var objectClassification = request.dto.Adapt<ObjectClassification>();
            objectClassification.ModificationDate = DateTime.UtcNow;

            // Persist
            _context.ObjectClassifications.Add(objectClassification);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Object classification created successfully"
            };
        }
    }
}
