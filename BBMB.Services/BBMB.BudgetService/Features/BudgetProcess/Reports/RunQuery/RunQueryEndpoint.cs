using BBMB.BudgetService.Features.BudgetProcess.Reports.RunQuery.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Reports.RunQuery
{
    public sealed class RunQueryEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/budget-reports/query",
                    async (RunQueryDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(await mediator.Send(new RunQueryCommand(dto), ct),
                            r => Results.Ok((object)r)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("RunBudgetReportQuery")
                .WithTags("BudgetReports")
                .Produces<Response<RunQueryResultDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status409Conflict);
        }
    }
}
