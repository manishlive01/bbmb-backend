using BBMB.BudgetService.Features.HierarchyManagement.CRManagements.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRManagements.Create
{
    public sealed record CreateCRManagementCommand(
        CreateCRManagementDto Dto
    ) : IRequest<Response>;
}
