using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Delete
{
    public sealed record DeleteCRSectionManagementCommand(int crSectionId) : IRequest<Response>;
}