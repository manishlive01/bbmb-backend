using BBMB.BudgetService.Features.EmployeeCharges.GetListByUser.Dtos;
using MediatR;
using System.Security.Claims;

namespace BBMB.BudgetService.Features.EmployeeCharges.GetListByUser
{
    public sealed record GetEmployeeChargeByUserQuery(
        ClaimsPrincipal User
    ) : IRequest<List<GetEmployeeChargeByUserResponseDto>>;
}