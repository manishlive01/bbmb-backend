using BBMB.BudgetService.Features.HierarchyManagement.Divisions.GetByCECR.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.HierarchyManagement.Divisions.GetByCECR
{
    public class GetDivisionByCECRHandler : IRequestHandler<GetDivisionByCECRQuery, List<GetDivisionByCECRResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetDivisionByCECRHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<List<GetDivisionByCECRResponseDto>> Handle(GetDivisionByCECRQuery request, CancellationToken ct)
        { 
            return await _context.Divisions.Include(d=>d.CRManagement).ThenInclude(d=>d.CEManagement)    
                .Where(d =>   d.CRId == request.crId)
                .Select(d => new GetDivisionByCECRResponseDto
                {
                    DivisionId = d.CRId,
                    DivisionCode = d.DivisionCode, 
                    DivisionDesc = d.DivisionDesc,
                    RegionCode = d.RegionCode,
                    PoolCode = d.PoolCode,
                    ModificationDate = d.ModificationDate, 
                    UserCode = d.UserCode,
                    CECode = d.CRManagement.CEManagement.CECode,
                    CRCode = d.CRManagement.CRCode
                })
                .ToListAsync(ct);
        }
    }
}