using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetBySubMajorHead.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetBySubMajorHead
{
    public class GetMinorHeadsBySubMajorHeadHandler : IRequestHandler<GetMinorHeadsBySubMajorHeadQuery, List<GetMinorHeadsBySubMajorHeadResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetMinorHeadsBySubMajorHeadHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<List<GetMinorHeadsBySubMajorHeadResponseDto>> Handle(GetMinorHeadsBySubMajorHeadQuery request, CancellationToken ct)
        {
            var minorHeads = await _context.MinorHeads
                .Include(m => m.MajorHead)
                .Include(m => m.SubMajorHead)
                .Where(m => m.SubMajorAccCode == request.SubMajorAccCode)
                .Select(m => new GetMinorHeadsBySubMajorHeadResponseDto
                {
                    MinorAccCode = m.MinorAccCode,
                    MinorAccDesc = m.MinorAccDesc,
                    MajorAccCode = m.MajorAccCode,
                    MajorAccDesc = m.MajorHead.MajorAccDesc,
                    SubMajorAccCode = m.SubMajorAccCode,
                    SubMajorAccDesc = m.SubMajorHead.SubMajorAccDesc,
                    ModificationDate = m.ModificationDate,
                    UserCode = m.UserCode
                })
                .ToListAsync(ct);

            return minorHeads;
        }
    }
}
