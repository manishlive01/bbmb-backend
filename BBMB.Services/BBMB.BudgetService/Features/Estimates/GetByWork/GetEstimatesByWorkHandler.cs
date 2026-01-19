using BBMB.BudgetService.Features.Estimates.GetList;
using BBMB.BudgetService.Features.Estimates.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Estimates.GetByWork
{
    public class GetEstimatesByWorkHandler : IRequestHandler<GetEstimatesByWorkQuery, PagedResponse<GetEstimateListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetEstimatesByWorkHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<GetEstimateListResponseDto>> Handle(GetEstimatesByWorkQuery request, CancellationToken ct)
        {
            var listQuery = new GetEstimateListQuery(
                request.PageNumber,
                request.PageSize,
                null,
                false,
                request.WorkCode,
                null,
                null
            );

            var handler = new GetEstimateListHandler(_context);
            return await handler.Handle(listQuery, ct);
        }
    }
}
