using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.Divisions.Delete
{
    public sealed record DeleteDivisionCommand(int Code) : IRequest<Response>;
}
