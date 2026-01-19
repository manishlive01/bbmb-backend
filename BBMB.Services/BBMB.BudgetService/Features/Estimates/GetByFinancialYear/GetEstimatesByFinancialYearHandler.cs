using BBMB.BudgetService.Features.Estimates.GetList;
using BBMB.BudgetService.Features.Estimates.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Estimates.GetByFinancialYear
{
    public class GetEstimatesByFinancialYearHandler : IRequestHandler<GetEstimatesByFinancialYearQuery, PagedResponse<GetEstimateListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetEstimatesByFinancialYearHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<GetEstimateListResponseDto>> Handle(GetEstimatesByFinancialYearQuery request, CancellationToken ct)
        {
            var listQuery = new GetEstimateListQuery(
                request.PageNumber,
                request.PageSize,
                null,
                false,
                null,
                null,
                request.FinYearEnd
            );

            var handler = new GetEstimateListHandler(_context);
            return await handler.Handle(listQuery, ct);
        }
    }
}
