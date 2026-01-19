using BBMB.BudgetService.Features.Estimates.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Estimates.Create
{
    public class CreateEstimateHandler : IRequestHandler<CreateEstimateCommand, Response>
    {
        private readonly BudgetContext _context;

        public CreateEstimateHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(CreateEstimateCommand request, CancellationToken ct)
        {
            // Check for duplicate composite key
            var exists = await _context.Estimates
                .AnyAsync(e => e.WorkCode == request.dto.WorkCode &&
                              e.EstimateCode == request.dto.EstimateCode, ct);

            if (exists)
                return new Response
                {
                    IsSucced = false,
                    Message = $"Estimate with work code '{request.dto.WorkCode}' and estimate code '{request.dto.EstimateCode}' already exists"
                };

            // Map and create
            var estimate = request.dto.Adapt<Estimate>();

            _context.Estimates.Add(estimate);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Estimate created successfully"
            };
        }
    }
}
