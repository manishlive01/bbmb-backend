using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.Delete
{
    public sealed record DeleteDivSectionManagementCommand(int divSectionId) : IRequest<Response>;
}