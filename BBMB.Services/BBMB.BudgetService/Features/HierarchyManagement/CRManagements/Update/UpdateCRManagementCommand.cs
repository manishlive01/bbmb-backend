using BBMB.BudgetService.Features.HierarchyManagement.CRManagements.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRManagements.Update
{
    public sealed record UpdateCRManagementCommand(
        int crId,
        UpdateCRManagementDto Dto
    ) : IRequest<Response>;
}
