using BBMB.BudgetService.Features.BudgetProcess.Settings.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Settings.GetInForce
{
    public sealed class GetSettingsInForceHandler(IBudgetSettingsReader reader, TimeProvider? clock = null)
        : IRequestHandler<GetSettingsInForceQuery, Response<IReadOnlyList<SettingInForceDto>>>
    {
        public async Task<Response<IReadOnlyList<SettingInForceDto>>> Handle(GetSettingsInForceQuery request, CancellationToken ct) =>
            new()
            {
                IsSucced = true,
                Message = "Settings in force",
                Data = await reader.GetInForceAsync(request.At ?? (clock ?? TimeProvider.System).GetUtcNow(), ct),
            };
    }
}
