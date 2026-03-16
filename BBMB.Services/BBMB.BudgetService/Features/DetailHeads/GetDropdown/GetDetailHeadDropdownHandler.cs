using BBMB.BudgetService.Features.DetailHeads.GetDropdown.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.DetailHeads.GetDropdown
{
    public class GetDetailHeadDropdownHandler
        : IRequestHandler<GetDetailHeadDropdownQuery, List<DetailHeadDropdownDto>>
    {
        private readonly BudgetContext _context;

        public GetDetailHeadDropdownHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<List<DetailHeadDropdownDto>> Handle(
            GetDetailHeadDropdownQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.DetailHeads.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.DetailAccCode))
            {
                query = query.Where(x => x.DetailAccCode.Contains(request.DetailAccCode));
            }

            if (!string.IsNullOrWhiteSpace(request.DetailAccDesc))
            {
                query = query.Where(x => x.DetailAccDesc.Contains(request.DetailAccDesc));
            }

            return await query
                .OrderBy(x => x.DetailAccCode)
                .Select(x => new DetailHeadDropdownDto
                {
                    DetailAccCode = x.DetailAccCode,
                    DetailAccDesc = x.DetailAccDesc
                })
                .ToListAsync(cancellationToken);
        }
    }
}