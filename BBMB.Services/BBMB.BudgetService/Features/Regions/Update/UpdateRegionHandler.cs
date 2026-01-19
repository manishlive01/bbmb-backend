using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Regions.Update
{
    public class UpdateRegionHandler : IRequestHandler<UpdateRegionCommand, Response>
    {
        private readonly BudgetContext _context;

        public UpdateRegionHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(UpdateRegionCommand request, CancellationToken ct)
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

            region.RegionDesc = request.dto.RegionDesc;
            region.UserCode = request.dto.UserCode;
            region.ModificationDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Region updated successfully"
            };
        }
    }
}
