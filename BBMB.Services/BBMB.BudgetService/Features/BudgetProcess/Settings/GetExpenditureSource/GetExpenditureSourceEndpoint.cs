using BBMB.BudgetService.Features.BudgetProcess.Settings.GetExpenditureSource.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Settings.GetExpenditureSource
{
    public sealed class GetExpenditureSourceEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budget-settings/expenditure-source",
                    async (long officeId, int finYear, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(await mediator.Send(new GetExpenditureSourceQuery(officeId, finYear), ct),
                            r => Results.Ok((object)r)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetExpenditureSource")
                .WithTags("BudgetSettings")
                .Produces<Response<ExpenditureSourceDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden);
        }
    }
}
