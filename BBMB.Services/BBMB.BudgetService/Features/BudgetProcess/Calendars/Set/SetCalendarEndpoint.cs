using BBMB.BudgetService.Features.BudgetProcess.Calendars.Set.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Calendars.Set
{
    public sealed class SetCalendarEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/budget-cycles/{cycleId:long}/calendar",
                    async (long cycleId, SetCalendarDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(await mediator.Send(new SetCalendarCommand(cycleId, dto), ct),
                            r => Results.Ok((object)r)))
                .RequireAuthorization("BudgetRight:F23.Set")
                .WithName("SetBudgetCalendar")
                .WithTags("BudgetCalendar")
                .Produces<Response>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
