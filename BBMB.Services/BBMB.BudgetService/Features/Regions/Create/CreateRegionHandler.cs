using BBMB.BudgetService.Features.Regions.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Regions.Create
{
    public class CreateRegionHandler
        : IRequestHandler<CreateRegionCommand, Response>
    {
        private readonly BudgetContext _context;

        public CreateRegionHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(
            CreateRegionCommand request,
            CancellationToken ct)
        {
            // 1️⃣ Map DTO → Entity
            var region = request.dto.Adapt<Region>();

            // 2️⃣ Duplicate check
            var exists = await _context.Regions
                .AnyAsync(x =>
                    x.RegionCode == region.RegionCode,
                    ct);

            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Region code already exists"
                };
            }

            // 3️⃣ System fields
            region.ModificationDate = DateTime.UtcNow;

            // 4️⃣ Persist
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
