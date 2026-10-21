using BBMB.BudgetService.Features.BudgetProcess.Calendars.Get.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Calendars.Get
{
    public sealed class GetCalendarEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budget-cycles/{cycleId:long}/calendar",
                    async (long cycleId, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(await mediator.Send(new GetCalendarQuery(cycleId), ct),
                            r => Results.Ok((object)r)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetBudgetCalendar")
                .WithTags("BudgetCalendar")
                .Produces<Response<CalendarViewDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound);
        }
    }
}
