using BBMB.BudgetService.Features.Regions.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Regions.Create
{
    public class CreateRegionHandler : IRequestHandler<CreateRegionCommand, Response>
    {
        private readonly BudgetContext _context;

        public CreateRegionHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(CreateRegionCommand request, CancellationToken ct)
        {
            var exists = await _context.Regions
                .AnyAsync(r => r.RegionCode == request.dto.RegionCode, ct);

            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Region with code '{request.dto.RegionCode}' already exists"
                };
            }

            var region = request.dto.Adapt<Region>();
            region.ModificationDate = DateTime.UtcNow;

            _context.Regions.Add(region);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Region created successfully"
            };
        }
    }
}
