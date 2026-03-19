using BBMB.BudgetService.Features.HierarchyManagement.LevelManagement.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.LevelManagement.GetList
{
    public sealed record GetLevelManagementListQuery() : IRequest<PagedResponse<GetLevelListResponseDto>>;
}
