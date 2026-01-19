using BBMB.BudgetService.Features.AccountingHeads.SubMinor.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinor.GetById
{
    public class GetSubMinorHeadByIdHandler : IRequestHandler<GetSubMinorHeadByIdQuery, GetSubMinorHeadByIdResponseDto>
    {
        private readonly BudgetContext _context;

        public GetSubMinorHeadByIdHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<GetSubMinorHeadByIdResponseDto> Handle(GetSubMinorHeadByIdQuery request, CancellationToken ct)
        {
            var subMinorHead = await _context.SubMinorHeads
                //.Include(s => s.MajorHead)
                //.Include(s => s.SubMajorHead)
                .Include(s => s.MinorHead)
                .FirstOrDefaultAsync(s => s.SubMinorAccCode == request.SubMinorAccCode, ct);

            if (subMinorHead == null)
                throw new KeyNotFoundException($"Sub-minor head with code '{request.SubMinorAccCode}' not found");

            return new GetSubMinorHeadByIdResponseDto
            {
                SubMinorAccCode = subMinorHead.SubMinorAccCode,
                SubMinorAccDesc = subMinorHead.SubMinorAccDesc,
                //MajorAccCode = subMinorHead.MajorAccCode,
                //MajorAccDesc = subMinorHead.MajorHead.MajorAccDesc,
                //SubMajorAccCode = subMinorHead.SubMajorAccCode,
                //SubMajorAccDesc = subMinorHead.SubMajorHead.SubMajorAccDesc,
                MinorAccCode = subMinorHead.MinorAccCode,
                MinorAccDesc = subMinorHead.MinorHead.MinorAccDesc,
                ModificationDate = subMinorHead.ModificationDate,
                UserCode = subMinorHead.UserCode
            };
        }
    }
}
