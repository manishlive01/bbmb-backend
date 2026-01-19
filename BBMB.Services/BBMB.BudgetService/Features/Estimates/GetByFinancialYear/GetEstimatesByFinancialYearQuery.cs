using BBMB.BudgetService.Features.Estimates.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Estimates.GetByFinancialYear
{
    public sealed record GetEstimatesByFinancialYearQuery(
        int FinYearEnd,
        int PageNumber,
        int PageSize
    ) : IRequest<PagedResponse<GetEstimateListResponseDto>>;
}
