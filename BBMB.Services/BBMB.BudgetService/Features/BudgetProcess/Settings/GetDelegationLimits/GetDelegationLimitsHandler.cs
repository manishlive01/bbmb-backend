using BBMB.BudgetService.Features.BudgetProcess.Settings.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Validators;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Settings.GetDelegationLimits
{
    /// <summary>Every T-19 row (the history is part of the record, DB-27), marked in force on the date asked.</summary>
    public sealed class GetDelegationLimitsHandler(IDelegationLimitReader reader, TimeProvider? clock = null)
        : IRequestHandler<GetDelegationLimitsQuery, Response<IReadOnlyList<DelegationLimitDto>>>
    {
        public async Task<Response<IReadOnlyList<DelegationLimitDto>>> Handle(GetDelegationLimitsQuery request, CancellationToken ct) =>
            new()
            {
                IsSucced = true,
                Message = "Delegation limits",
                Data = await reader.GetAllAsync(request.On ?? RupeeRuleExtensions.TodayIst(clock), ct),
            };
    }
}
