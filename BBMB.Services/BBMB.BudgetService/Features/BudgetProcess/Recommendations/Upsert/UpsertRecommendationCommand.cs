using BBMB.BudgetService.Features.BudgetProcess.Recommendations.Upsert.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Recommendations.Upsert
{
    /// <summary>Success: <c>Response&lt;long&gt;</c> with the T-37 row id.</summary>
    public sealed record UpsertRecommendationCommand(long CycleId, UpsertRecommendationDto Dto) : IRequest<Response<long>>;
}
