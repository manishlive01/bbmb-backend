using BBMB.BudgetService.Features.Units.GetByPowerIrrigation.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Units.GetByPowerIrrigation
{
    public class GetUnitsByPowerIrrigationHandler : IRequestHandler<GetUnitsByPowerIrrigationQuery, Response<List<GetUnitsByPowerIrrigationResponseDto>>>
    {
        private readonly BudgetContext _context;

        public GetUnitsByPowerIrrigationHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response<List<GetUnitsByPowerIrrigationResponseDto>>> Handle(GetUnitsByPowerIrrigationQuery request, CancellationToken ct)
        {
            var units = await _context.Units
                .Include(u => u.Project)
                .Include(u => u.PowerIrrigation)
                .Where(u => u.IrrPwrCode == request.IrrPwrCode)
                .OrderBy(u => u.UnitCode)
                .Select(u => new GetUnitsByPowerIrrigationResponseDto
                {
                    UnitCode = u.UnitCode,
                    UnitDesc = u.UnitDesc,
                    ProjCode = u.ProjCode,
                    ProjectDescription = u.Project != null ? u.Project.ProjDesc : string.Empty,
                    IrrPwrCode = u.IrrPwrCode,
                    PowerIrrigationDescription = u.PowerIrrigation != null ? u.PowerIrrigation.IrrPwrDesc : string.Empty,
                    ModificationDate = u.ModificationDate,
                    UserCode = u.UserCode
                })
                .ToListAsync(ct);

            return new Response<List<GetUnitsByPowerIrrigationResponseDto>>
            {
                IsSucced = true,
                Message = $"Retrieved {units.Count} units for power/irrigation code '{request.IrrPwrCode}'",
                Data = units
            };
        }
    }
}
