using BBMB.BudgetService.Features.PublicMasters.AppointmentTypes.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.PublicMasters.AppointmentTypes
{
    public class GetAppointmentTypeListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/Get-AppointmentType-List", async (
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new GetAppointmentTypeListCommand(), ct);

                return Results.Ok(result);
            })
            .Produces<List<GetAppointmentTypeListResponseDto>>(StatusCodes.Status200OK)
            .WithSummary("Get Appointment Type List")
            .WithTags("Public");
        }
    }
}
