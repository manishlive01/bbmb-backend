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
            return await _context.Divisions
                .Where(d => d.CECode == request.CECode && d.CRCode == request.CRCode)
                .Select(d => new GetDivisionByCECRResponseDto
                {
                    DivisionCode = d.DivisionCode,
                    DivSection = d.DivSection,
                    DivisionDesc = d.DivisionDesc,
                    RegionCode = d.RegionCode,
                    PoolCode = d.PoolCode,
                    ModificationDate = d.ModificationDate,
                    CECode = d.CECode,
                    CRCode = d.CRCode,
                    UserCode = d.UserCode
                })
                .ToListAsync(ct);
        }
    }
}