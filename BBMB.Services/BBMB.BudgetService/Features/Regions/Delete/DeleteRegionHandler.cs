using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Regions.Delete
{
    public class DeleteRegionHandler : IRequestHandler<DeleteRegionCommand, Response>
    {
        private readonly BudgetContext _context;

        public DeleteRegionHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(DeleteRegionCommand request, CancellationToken ct)
        {
            var region = await _context.Regions
                .FirstOrDefaultAsync(r => r.RegionCode == request.Code, ct);

            if (region == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Region with code '{request.Code}' not found"
                };
            }

            _context.Regions.Remove(region);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Region deleted successfully"
            };
        }
    }
}
