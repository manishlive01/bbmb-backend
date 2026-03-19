using BBMB.BudgetService.Features.HierarchyManagement.Divisions.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.Divisions.Update
{
    public sealed record UpdateDivisionCommand(int Code, UpdateDivisionDto dto) : IRequest<Response>;
}
