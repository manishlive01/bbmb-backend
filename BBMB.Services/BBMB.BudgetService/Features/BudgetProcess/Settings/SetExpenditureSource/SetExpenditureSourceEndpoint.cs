using BBMB.BudgetService.Features.BudgetProcess.Settings.SetExpenditureSource.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Settings.SetExpenditureSource
{
    public sealed class SetExpenditureSourceEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // Settings.ExpenditureSource: the IT Cell role alone (design §4.4, BUD-025).
            app.MapPost("/api/budget-settings/expenditure-source",
                    async (SetExpenditureSourceDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(await mediator.Send(new SetExpenditureSourceCommand(dto), ct),
                            r => Results.Ok((object)r)))
                .RequireAuthorization("BudgetRight:Settings.ExpenditureSource")
                .WithName("SetExpenditureSource")
                .WithTags("BudgetSettings")
                .Produces<Response>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
