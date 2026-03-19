using BBMB.BudgetService.Features.HierarchyManagement.Divisions.GetById.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.Divisions.GetById
{
    public sealed record GetDivisionByIdQuery(int Code) : IRequest<GetDivisionByIdResponseDto>;
}
