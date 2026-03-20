using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.CEManagements.Delete
{
    public sealed record DeleteCEManagementCommand(int ceId) : IRequest<Response>;
}
