using BBMB.BudgetService.Features.AccountingHeads.SubMinor.GetByMinorHead.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinor.GetByMinorHead
{
    public class GetSubMinorHeadsByMinorHeadHandler : IRequestHandler<GetSubMinorHeadsByMinorHeadQuery, List<GetSubMinorHeadsByMinorHeadResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetSubMinorHeadsByMinorHeadHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<List<GetSubMinorHeadsByMinorHeadResponseDto>> Handle(GetSubMinorHeadsByMinorHeadQuery request, CancellationToken ct)
        {
            var subMinorHeads = await _context.SubMinorHeads
                //.Include(s => s.MajorHead)
                //.Include(s => s.SubMajorHead)
                .Include(s => s.MinorHead)
                .Where(s => s.MinorAccCode == request.MinorAccCode)
                .Select(s => new GetSubMinorHeadsByMinorHeadResponseDto
                {
                    SubMinorAccCode = s.SubMinorAccCode,
                    SubMinorAccDesc = s.SubMinorAccDesc,
                    //MajorAccCode = s.MajorAccCode,
                    //MajorAccDesc = s.MajorHead.MajorAccDesc,
                    //SubMajorAccCode = s.SubMajorAccCode,
                    //SubMajorAccDesc = s.SubMajorHead.SubMajorAccDesc,
                    MinorAccCode = s.MinorAccCode,
                    MinorAccDesc = s.MinorHead.MinorAccDesc,
                    ModificationDate = s.ModificationDate,
                    UserCode = s.UserCode
                })
                .ToListAsync(ct);

            return subMinorHeads;
        }
    }
}
