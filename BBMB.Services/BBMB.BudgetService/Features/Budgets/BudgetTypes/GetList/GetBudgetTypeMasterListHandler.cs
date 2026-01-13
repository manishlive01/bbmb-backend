using BBMB.BudgetService.Features.Budgets.BudgetTypes.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Budgets.BudgetTypes.GetList
{
    public class GetBudgetTypeMasterListHandler
        : IRequestHandler<GetBudgetTypeMasterListCommand, List<GetBudgetTypeMasterListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetBudgetTypeMasterListHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<List<GetBudgetTypeMasterListResponseDto>> Handle(
            GetBudgetTypeMasterListCommand request,
            CancellationToken ct)
        {
            return await _context.BudgetTypeMasters
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.BudgetTypeName)
                .Select(x => new GetBudgetTypeMasterListResponseDto
                {
                    BudgetTypeMasterId = x.BudgetTypeMasterId,
                    BudgetTypeCode = x.BudgetTypeCode,
                    BudgetTypeName = x.BudgetTypeName,
                    Description = x.Description,
                    IsActive = x.IsActive
                })
                .ToListAsync(ct);
        }
    }
}
