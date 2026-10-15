using BBMB.BudgetService.Features.BudgetProcess.Settings.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Settings.GetDelegationLimits
{
    /// <summary><paramref name="On"/> null means today in IST.</summary>
    public sealed record GetDelegationLimitsQuery(DateOnly? On) : IRequest<Response<IReadOnlyList<DelegationLimitDto>>>;
}
