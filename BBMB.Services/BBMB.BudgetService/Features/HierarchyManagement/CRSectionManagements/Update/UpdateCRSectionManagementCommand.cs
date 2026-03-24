using BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Update
{
    public sealed record UpdateCRSectionManagementCommand(UpdateCRSectionManagementDto dto)
        : IRequest<Response>;
}