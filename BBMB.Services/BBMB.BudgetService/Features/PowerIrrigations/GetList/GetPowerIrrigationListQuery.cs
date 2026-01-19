using BBMB.BudgetService.Features.PowerIrrigations.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.PowerIrrigations.GetList
{
    public sealed record GetPowerIrrigationListQuery(
        int PageNumber = 1,
        int PageSize = 10,
        string? SortBy = null,
        bool SortDescending = false,
        string? IrrPwrCode = null,
        string? IrrPwrDesc = null,
        string? UserCode = null,
        DateTime? ModificationDateFrom = null,
        DateTime? ModificationDateTo = null
    ) : IRequest<PagedResponse<GetPowerIrrigationListResponseDto>>;
}
