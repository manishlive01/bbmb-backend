using BBMB.BudgetService.Features.Budgets.BudgetTypes.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Budgets.BudgetTypes.GetById
{
    public class GetBudgetTypeMasterByIdHandler
        : IRequestHandler<GetBudgetTypeMasterByIdCommand, GetBudgetTypeMasterByIdResponseDto?>
    {
        private readonly BudgetContext _context;

        public GetBudgetTypeMasterByIdHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<GetBudgetTypeMasterByIdResponseDto?> Handle(
            GetBudgetTypeMasterByIdCommand request,
            CancellationToken ct)
        {
            return await _context.BudgetTypeMasters
                .AsNoTracking()
                .Where(x =>
                    x.BudgetTypeMasterId == request.Request.Id &&
                    !x.IsDeleted)
                .Select(x => new GetBudgetTypeMasterByIdResponseDto
                {
                    BudgetTypeCode = x.BudgetTypeCode,
                    BudgetTypeName = x.BudgetTypeName,
                    Description = x.Description,
                    IsActive = x.IsActive
                })
                .FirstOrDefaultAsync(ct);
        }
    }
}
