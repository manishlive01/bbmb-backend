using BBMB.BudgetService.Features.BudgetProcess.Recommendations.GetRows.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Recommendations.GetRows
{
    public sealed record GetRecommendationRowsQuery(long CycleId, string Category) : IRequest<Response<List<RecommendationRowDto>>>;
}
