using BBMB.BudgetService.Features.BudgetProcess.Reports.RunQuery.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Reports.RunQuery
{
    /// <summary>Design §8.8 (<c>POST /api/budget-reports/query</c>, F-26).</summary>
    public sealed record RunQueryCommand(RunQueryDto Dto) : IRequest<Response<RunQueryResultDto>>;
}
