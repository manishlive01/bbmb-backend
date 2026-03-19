using BBMB.BudgetService.Features.HierarchyManagement.CEManagements.GetById.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.CEManagements.GetById
{
    public sealed record GetCEManagementByIdQuery(int ceCode)
        : IRequest<Response<GetCEManagementByIdResponseDto>>;
}