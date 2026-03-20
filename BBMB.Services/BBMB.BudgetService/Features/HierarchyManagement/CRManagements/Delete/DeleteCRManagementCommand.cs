using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRManagements.Delete
{
    public sealed record DeleteCRManagementCommand(int crId)
        : IRequest<Response>;
}
