using BBMB.BudgetService.Features.BudgetProcess.Reports.GetQueryFields.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Reports.GetQueryFields
{
    /// <summary>Design §8.8 (<c>GET /api/budget-reports/fields</c>, F-26, BUD-220): the business-name field catalogue.</summary>
    public sealed record GetQueryFieldsQuery : IRequest<Response<List<QueryFieldDto>>>;
}
