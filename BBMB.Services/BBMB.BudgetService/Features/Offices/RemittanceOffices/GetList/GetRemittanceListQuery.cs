using BBMB.BudgetService.Features.Offices.RemittanceOffices.GetList.Dtos;
using BBMB.BudgetService.Features.Pools.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Offices.RemittanceOffices.GetList
{
    
    public sealed record GetRemittanceListQuery(
       int PageNumber = 1,
       int PageSize = 10,
       string? SortBy = null,
       bool SortDescending = false,
       string? ROCode = null,
       string? RODesc = null,
       string? UserCode = null,
       DateTime? ModificationDateFrom = null,
       DateTime? ModificationDateTo = null
   ) : IRequest<PagedResponse<GetRemittanceListResponseDto>>;
}
