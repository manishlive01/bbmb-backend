using BBMB.BudgetService.Features.BudgetProcess.Settings.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Settings.GetInForce
{
    /// <summary><paramref name="At"/> null means now.</summary>
    public sealed record GetSettingsInForceQuery(DateTimeOffset? At) : IRequest<Response<IReadOnlyList<SettingInForceDto>>>;
}
