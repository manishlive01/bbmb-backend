using BBMB.BudgetService.Features.Units.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Units.GetById
{
    public class GetUnitByIdHandler : IRequestHandler<GetUnitByIdQuery, GetUnitByIdResponseDto>
    {
        private readonly BudgetContext _context;

        public GetUnitByIdHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<GetUnitByIdResponseDto> Handle(GetUnitByIdQuery request, CancellationToken ct)
        {
            var unit = await _context.Units
                .Include(u => u.Project)
                .Include(u => u.PowerIrrigation)
                .FirstOrDefaultAsync(u => u.UnitCode == request.UnitCode, ct);

            if (unit == null)
            {
                throw new KeyNotFoundException($"Unit with code '{request.UnitCode}' not found");
            }

            return new GetUnitByIdResponseDto
            {
                UnitCode = unit.UnitCode,
                UnitDesc = unit.UnitDesc,
                ProjCode = unit.ProjCode,
                ProjectDescription = unit.Project?.ProjDesc ?? string.Empty,
                IrrPwrCode = unit.IrrPwrCode,
                PowerIrrigationDescription = unit.PowerIrrigation?.IrrPwrDesc ?? string.Empty,
                ModificationDate = unit.ModificationDate,
                UserCode = unit.UserCode
            };
        }
    }
}
