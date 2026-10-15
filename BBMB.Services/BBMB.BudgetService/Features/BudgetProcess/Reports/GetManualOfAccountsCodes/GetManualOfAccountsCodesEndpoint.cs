using BBMB.BudgetService.Features.BudgetProcess.Reports.GetManualOfAccountsCodes.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Reports.GetManualOfAccountsCodes
{
    public sealed class GetManualOfAccountsCodesEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/manual-of-accounts-codes",
                    async (IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(await mediator.Send(new GetManualOfAccountsCodesQuery(), ct),
                            r => Results.Ok((object)r)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetManualOfAccountsCodes")
                .WithTags("BudgetReports")
                .Produces<Response<ManualOfAccountsResultDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden);
        }
    }
}
