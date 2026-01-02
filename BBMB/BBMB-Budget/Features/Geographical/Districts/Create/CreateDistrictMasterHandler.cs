using BBMB_Budget.Features.Geographical.Districts.Entities;
using BBMB_Budget.Infrastructure.Persistence;
using BBMB_Budget.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB_Budget.Features.Geographical.Districts.Create
{
    public class CreateDistrictMasterHandler
        : IRequestHandler<CreateDistrictMasterCommand, Response>
    {
        private readonly BBMBContext _context;

        public CreateDistrictMasterHandler(BBMBContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(
            CreateDistrictMasterCommand request,
            CancellationToken ct)
        {
            // 1️⃣ Map DTO → Entity
            var district = request.dto.Adapt<DistrictMaster>();

            // 2️⃣ Duplicate check
            var exists = await _context.DistrictMasters
                .AnyAsync(x =>
                    x.StateMasterId == district.StateMasterId &&
                    x.DistrictCode == district.DistrictCode &&
                    x.IsDeleted == false,
                    ct);

            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "District code already exists"
                };
            }

            // 3️⃣ System fields
            district.IsDeleted = false;
            district.CreatedAt = DateTime.UtcNow;

            // 4️⃣ Persist
            _context.DistrictMasters.Add(district);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "District created successfully"
            };
        }
    }
}
