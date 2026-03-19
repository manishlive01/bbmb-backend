using BBMB.BudgetService.Features.Offices.OfficeManagements.GetListByFilter.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Offices.OfficeManagements.GetListByFilter
{
    public class GetOfficeManagementByFilterHandler
        : IRequestHandler<GetOfficeManagementByFilterQuery, List<GetOfficeManagementByFilterDto>>
    {
        private readonly BudgetContext _context;

        public GetOfficeManagementByFilterHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<List<GetOfficeManagementByFilterDto>> Handle(
            GetOfficeManagementByFilterQuery request,
            CancellationToken ct)
        {
            var query = _context.OfficeManagements.AsQueryable();

            // 🔍 Apply filters (ONLY these 3)
            if (request.CECode.HasValue)
                query = query.Where(x => x.CECode == request.CECode);

            if (request.CRCode.HasValue)
                query = query.Where(x => x.CRCode == request.CRCode);

            if (request.DivisionCode.HasValue)
                query = query.Where(x => x.DivisionCode == request.DivisionCode);

            var data = await query.ToListAsync(ct);

            // 🔥 Lookup optimization
            var ceCodes = data.Select(x => x.CECode).Distinct().ToList();
            var crCodes = data.Where(x => x.CRCode.HasValue).Select(x => x.CRCode!.Value).Distinct().ToList();
            var divCodes = data.Where(x => x.DivisionCode.HasValue).Select(x => x.DivisionCode!.Value).Distinct().ToList();
            var levelIds = data.Select(x => x.LevelId).Distinct().ToList();
            var ces = await _context.CEManagements
                .Where(x => ceCodes.Contains(x.CECode))
                .ToDictionaryAsync(x => x.CECode, ct);

            var crs = await _context.CRManagements
                .Where(x => crCodes.Contains(x.CRCode))
                .ToDictionaryAsync(x => x.CRCode, ct);

            var divs = await _context.Divisions
                .Where(x => divCodes.Contains(x.DivisionCode))
                .ToDictionaryAsync(x => x.DivisionCode, ct);
            var levels = await _context.Levels
                .Where(x => levelIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, ct);
            // 🔁 Mapping
            return data.Select(x => new GetOfficeManagementByFilterDto
            {
                OfficeCode = x.OfficeCode,
                OfficeDesc = x.OfficeDesc,

                CECode = x.CECode,
                CEName = ces.ContainsKey(x.CECode)
                    ? ces[x.CECode].Name
                    : "",

                CRCode = x.CRCode,
                CRName = x.CRCode.HasValue && crs.ContainsKey(x.CRCode.Value)
                    ? crs[x.CRCode.Value].Name
                    : "",

                DivisionCode = x.DivisionCode,
                DivName = x.DivisionCode.HasValue && divs.ContainsKey(x.DivisionCode.Value)
                    ? divs[x.DivisionCode.Value].DivisionDesc
                    : "",
                LevelId = x.LevelId,
                LevelName = levels.ContainsKey(x.LevelId)
                    ? levels[x.LevelId].Name
                    : ""
            }).ToList();
        }
    }
}