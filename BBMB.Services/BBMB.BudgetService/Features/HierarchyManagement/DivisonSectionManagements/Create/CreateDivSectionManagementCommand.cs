using BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.Create
{
    public sealed record CreateDivSectionManagementCommand(CreateDivSectionManagementDto dto) : IRequest<Response>;
}