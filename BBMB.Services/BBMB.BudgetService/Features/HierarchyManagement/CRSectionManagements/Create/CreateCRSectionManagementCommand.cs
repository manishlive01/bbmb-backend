using BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Create
{
    public sealed record CreateCRSectionManagementCommand(CreateCRSectionManagementDto dto) : IRequest<Response>;
}