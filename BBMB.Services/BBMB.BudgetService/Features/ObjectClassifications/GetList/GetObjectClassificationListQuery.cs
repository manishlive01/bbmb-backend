using BBMB.BudgetService.Features.ObjectClassifications.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.ObjectClassifications.GetList
{
    public sealed record GetObjectClassificationListQuery(
        int PageNumber = 1,
        int PageSize = 10,
        string? SortBy = null,
        bool SortDescending = false,
        string? ObjectAccCode = null,
        string? ObjectAccDesc = null,
        string? UserCode = null,
        DateTime? ModificationDateFrom = null,
        DateTime? ModificationDateTo = null
    ) : IRequest<PagedResponse<GetObjectClassificationListResponseDto>>;
}
