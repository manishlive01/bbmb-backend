using BBMB.BudgetService.Features.BudgetProcess.Reports.GetQueryFields.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Reports.GetQueryFields
{
    public sealed class GetQueryFieldsEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budget-reports/fields",
                    async (IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(await mediator.Send(new GetQueryFieldsQuery(), ct),
                            r => Results.Ok((object)r)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetBudgetReportQueryFields")
                .WithTags("BudgetReports")
                .Produces<Response<List<QueryFieldDto>>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden);
        }
    }
}
