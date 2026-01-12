using BBMB.BudgetService.Infrastructure.Persistence;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Geographical.Districts.Delete
{
    public class DeleteDistrictMasterHandler
        : IRequestHandler<DeleteDistrictMasterCommand, Response>
    {
        private readonly BBMBContext _context;

        public DeleteDistrictMasterHandler(BBMBContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(
            DeleteDistrictMasterCommand request,
            CancellationToken ct)
        {
            var id = request.Request.Id;

            // 1️⃣ Fetch entity
            var District = await _context.DistrictMasters
                .FirstOrDefaultAsync(x =>
                    x.DistrictMasterId == id &&
                    x.IsDeleted == false,
                    ct);

            if (District == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "District not found"
                };
            }

            // 2️⃣ Soft delete
            District.IsDeleted = true;
            District.DeletedAt = DateTime.UtcNow;

            // Optional: deactivate
            District.IsActive = false;

            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "District deleted successfully"
            };
        }
    }
}
