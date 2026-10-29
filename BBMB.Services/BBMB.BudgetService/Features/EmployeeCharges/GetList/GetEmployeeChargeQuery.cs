using BBMB.BudgetService.Features.EmployeeCharges.GetList.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.EmployeeCharges.GetList
{
    public sealed record GetEmployeeChargeQuery(
    ) : IRequest<List<GetEmployeeChargeResponseDto>>;
}