using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Estimates.Delete
{
    public class DeleteEstimateHandler : IRequestHandler<DeleteEstimateCommand, Response>
    {
        private readonly BudgetContext _context;

        public DeleteEstimateHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(DeleteEstimateCommand request, CancellationToken ct)
        {
            var estimate = await _context.Estimates
                .FirstOrDefaultAsync(e => e.WorkCode == request.WorkCode && 
                                         e.EstimateCode == request.EstimateCode, ct);

            if (estimate == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Estimate with work code '{request.WorkCode}' and estimate code '{request.EstimateCode}' not found"
                };
            }

            _context.Estimates.Remove(estimate);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Estimate deleted successfully"
            };
        }
    }
}
