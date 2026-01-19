using BBMB.BudgetService.Features.Regions.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Regions.GetById
{
    public class GetRegionByIdHandler : IRequestHandler<GetRegionByIdQuery, GetRegionByIdResponseDto>
    {
        private readonly BudgetContext _context;

        public GetRegionByIdHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<GetRegionByIdResponseDto> Handle(GetRegionByIdQuery request, CancellationToken ct)
        {
            var region = await _context.Regions
                .FirstOrDefaultAsync(r => r.RegionCode == request.Code, ct);

            if (region == null)
            {
                throw new KeyNotFoundException($"Region with code '{request.Code}' not found");
            }

            return new GetRegionByIdResponseDto
            {
                RegionCode = region.RegionCode,
                RegionDesc = region.RegionDesc,
                ModificationDate = region.ModificationDate,
                UserCode = region.UserCode
            };
        }
    }
}
