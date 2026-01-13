using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Geographical.Districts.Update
{
    public class UpdateDistrictMasterHandler
        : IRequestHandler<UpdateDistrictMasterCommand, Response>
    {
        private readonly BudgetContext _context;

        public UpdateDistrictMasterHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(
            UpdateDistrictMasterCommand request,
            CancellationToken ct)
        {
            var dto = request.dto;

            // 1️⃣ Fetch existing District
            var district = await _context.DistrictMasters
                .FirstOrDefaultAsync(x =>
                    x.DistrictMasterId == dto.DistrictMasterId &&
                    x.IsDeleted == false,
                    ct);

            if (district == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "District not found"
                };
            }

            // 2️⃣ Check duplicate DistrictCode (excluding self)
            var duplicate = await _context.DistrictMasters
                .AnyAsync(x =>
                    x.StateMasterId == dto.StateMasterId &&
                    x.DistrictCode == dto.DistrictCode &&
                    x.DistrictMasterId != dto.DistrictMasterId &&
                    x.IsDeleted == false,
                    ct);

            if (duplicate)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "District code already exists"
                };
            }

            // 3️⃣ Map updated fields
            dto.Adapt(district);

            // 4️⃣ System fields
            district.UpdatedAt = DateTime.UtcNow;

            // 5️⃣ Save
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "District updated successfully"
            };
        }
    }
}
