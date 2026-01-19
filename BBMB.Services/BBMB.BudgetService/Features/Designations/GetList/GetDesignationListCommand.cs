using BBMB.BudgetService.Features.Designations.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Designations.GetList
{
    public sealed record GetDesignationListCommand(
        GetDesignationListRequestDto Request
    ) : IRequest<PagedResponse<GetDesignationListResponseDto>>;
}
