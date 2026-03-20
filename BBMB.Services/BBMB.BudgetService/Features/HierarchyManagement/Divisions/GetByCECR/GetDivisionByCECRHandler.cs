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
            var cr = await _context.CRManagements.Include(d => d.CEManagement).FirstOrDefaultAsync(d=>d.CEManagement.Id==request.ceId&&d.Id==request.crId);
            return await _context.Divisions.Include(d=>d.DivSections)
                .Where(d =>   d.CRId == cr.Id)
                .Select(d => new GetDivisionByCECRResponseDto
                {
                    DivisionCode = d.DivisionCode, 
                    DivisionDesc = d.DivisionDesc,
                    RegionCode = d.RegionCode,
                    PoolCode = d.PoolCode,
                    ModificationDate = d.ModificationDate, 
                    UserCode = d.UserCode
                })
                .ToListAsync(ct);
        }
    }
}