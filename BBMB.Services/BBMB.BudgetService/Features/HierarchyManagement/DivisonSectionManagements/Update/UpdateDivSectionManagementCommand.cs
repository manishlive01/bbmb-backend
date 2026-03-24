using BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.Update
{
    public sealed record UpdateDivSectionManagementCommand(UpdateDivSectionManagementDto dto)
        : IRequest<Response>;
}