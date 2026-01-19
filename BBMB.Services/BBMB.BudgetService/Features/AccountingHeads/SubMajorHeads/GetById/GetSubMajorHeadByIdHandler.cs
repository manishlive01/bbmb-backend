using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetById
{
    public class GetSubMajorHeadByIdHandler : IRequestHandler<GetSubMajorHeadByIdQuery, GetSubMajorHeadByIdResponseDto>
    {
        private readonly BudgetContext _context;
        
        public GetSubMajorHeadByIdHandler(BudgetContext context)
        {
            _context = context;
        }
        
        public async Task<GetSubMajorHeadByIdResponseDto> Handle(GetSubMajorHeadByIdQuery request, CancellationToken ct)
        {
            var subMajorHead = await _context.SubMajorHeads
                .Include(s => s.MajorHead)
                .FirstOrDefaultAsync(s => s.SubMajorAccCode == request.SubMajorAccCode, ct);
                
            if (subMajorHead == null)
                throw new KeyNotFoundException($"Sub-major head with code '{request.SubMajorAccCode}' not found");
            
            return new GetSubMajorHeadByIdResponseDto
            {
                SubMajorAccCode = subMajorHead.SubMajorAccCode,
                SubMajorAccDesc = subMajorHead.SubMajorAccDesc,
                MajorAccCode = subMajorHead.MajorAccCode,
                MajorAccDesc = subMajorHead.MajorHead.MajorAccDesc,
                ModificationDate = subMajorHead.ModificationDate,
                UserCode = subMajorHead.UserCode
            };
        }
    }
}
