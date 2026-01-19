using BBMB.BudgetService.Features.MinorHeads.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.MinorHeads.GetById
{
    public class GetMinorHeadByIdHandler : IRequestHandler<GetMinorHeadByIdQuery, GetMinorHeadByIdResponseDto>
    {
        private readonly BudgetContext _context;

        public GetMinorHeadByIdHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<GetMinorHeadByIdResponseDto> Handle(GetMinorHeadByIdQuery request, CancellationToken ct)
        {
            var minorHead = await _context.MinorHeads
                .Include(m => m.MajorHead)
                .Include(m => m.SubMajorHead)
                .FirstOrDefaultAsync(m => m.MinorAccCode == request.MinorAccCode, ct);

            if (minorHead == null)
                throw new KeyNotFoundException($"Minor head with code '{request.MinorAccCode}' not found");

            return new GetMinorHeadByIdResponseDto
            {
                MinorAccCode = minorHead.MinorAccCode,
                MinorAccDesc = minorHead.MinorAccDesc,
                MajorAccCode = minorHead.MajorAccCode,
                MajorAccDesc = minorHead.MajorHead.MajorAccDesc,
                SubMajorAccCode = minorHead.SubMajorAccCode,
                SubMajorAccDesc = minorHead.SubMajorHead.SubMajorAccDesc,
                ModificationDate = minorHead.ModificationDate,
                UserCode = minorHead.UserCode
            };
        }
    }
}
