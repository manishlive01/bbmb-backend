using BBMB.BudgetService.Features.Budgets.GetById.Dtos;
using BBMB.BudgetService.Features.CEManagements.GetById.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.CEManagements.GetById
{
    public sealed record GetCEManagementByIdQuery(int Id)
        : IRequest<GetCEManagementByIdResponseDto>;
}
