using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetByMajorHead.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetByMajorHead
{
    public class GetSubMajorHeadsByMajorHeadHandler : IRequestHandler<GetSubMajorHeadsByMajorHeadQuery, List<GetSubMajorHeadsByMajorHeadResponseDto>>
    {
        private readonly BudgetContext _context;
        
        public GetSubMajorHeadsByMajorHeadHandler(BudgetContext context)
        {
            _context = context;
        }
        
        public async Task<List<GetSubMajorHeadsByMajorHeadResponseDto>> Handle(GetSubMajorHeadsByMajorHeadQuery request, CancellationToken ct)
        {
            var subMajorHeads = await _context.SubMajorHeads
                .Include(s => s.MajorHead)
                .Where(s => s.MajorAccCode == request.MajorAccCode)
                .Select(s => new GetSubMajorHeadsByMajorHeadResponseDto
                {
                    SubMajorAccCode = s.SubMajorAccCode,
                    SubMajorAccDesc = s.SubMajorAccDesc,
                    MajorAccCode = s.MajorAccCode,
                    MajorAccDesc = s.MajorHead.MajorAccDesc,
                    ModificationDate = s.ModificationDate,
                    UserCode = s.UserCode
                })
                .ToListAsync(ct);
            
            return subMajorHeads;
        }
    }
}
