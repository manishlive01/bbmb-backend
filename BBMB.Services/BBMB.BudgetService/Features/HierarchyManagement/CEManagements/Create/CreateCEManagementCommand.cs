using BBMB.BudgetService.Features.HierarchyManagement.CEManagements.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.CEManagements.Create
{
    public sealed record CreateCEManagementCommand(CreateCEManagementDto dto) : IRequest<Response>;
}
