using BBMB.BudgetService.Features.HierarchyManagement.CRManagements.GetById.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRManagements.GetById
{
    public sealed record GetCRManagementByIdQuery(int crCode)
        : IRequest<GetCRManagementByIdResponseDto>;
}
