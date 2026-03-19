using BBMB.BudgetService.Features.HierarchyManagement.CEManagements.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.CEManagements.Update
{
    public sealed record UpdateCEManagementCommand(
        int ceCode,
        UpdateCEManagementDto Dto
    ) : IRequest<Response>;
}
