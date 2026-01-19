using BBMB.BudgetService.Features.Units.GetByProject.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Units.GetByProject
{
    public class GetUnitsByProjectHandler : IRequestHandler<GetUnitsByProjectQuery, Response<List<GetUnitsByProjectResponseDto>>>
    {
        private readonly BudgetContext _context;

        public GetUnitsByProjectHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response<List<GetUnitsByProjectResponseDto>>> Handle(GetUnitsByProjectQuery request, CancellationToken ct)
        {
            var units = await _context.Units
                .Include(u => u.Project)
                .Include(u => u.PowerIrrigation)
                .Where(u => u.ProjCode == request.ProjCode)
                .OrderBy(u => u.UnitCode)
                .Select(u => new GetUnitsByProjectResponseDto
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

            return new Response<List<GetUnitsByProjectResponseDto>>
            {
                IsSucced = true,
                Message = $"Retrieved {units.Count} units for project '{request.ProjCode}'",
                Data = units
            };
        }
    }
}
