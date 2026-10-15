using BBMB.BudgetService.Features.BudgetProcess.Settings.CreateDelegationLimit.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Settings.CreateDelegationLimit
{
    public sealed record CreateDelegationLimitCommand(CreateDelegationLimitDto Dto) : IRequest<Response<long>>;
}
