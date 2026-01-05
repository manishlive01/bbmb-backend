using BBMB_Budget.Features.Budgets.BudgetTypes.GetList.Dtos;
using BBMB_Budget.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB_Budget.Features.Budgets.BudgetTypes.GetList
{
    public class GetBudgetTypeMasterListHandler
        : IRequestHandler<GetBudgetTypeMasterListCommand, List<GetBudgetTypeMasterListResponseDto>>
    {
        private readonly BBMBContext _context;

        public GetBudgetTypeMasterListHandler(BBMBContext context)
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
