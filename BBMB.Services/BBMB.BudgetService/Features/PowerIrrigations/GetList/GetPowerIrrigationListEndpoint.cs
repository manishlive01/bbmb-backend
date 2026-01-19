using BBMB.BudgetService.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.PowerIrrigations.GetList
{
    public class GetPowerIrrigationListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/power-irrigation", async (
                IMediator mediator,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? sortBy = null,
                [FromQuery] bool sortDescending = false,
                [FromQuery] string? irrPwrCode = null,
                [FromQuery] string? irrPwrDesc = null,
                [FromQuery] string? userCode = null,
                [FromQuery] DateTime? modificationDateFrom = null,
                [FromQuery] DateTime? modificationDateTo = null) =>
            {
                var query = new GetPowerIrrigationListQuery(
                    pageNumber, pageSize, sortBy, sortDescending,
                    irrPwrCode, irrPwrDesc, userCode,
                    modificationDateFrom, modificationDateTo);

                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetPowerIrrigationList")
            .WithTags("PowerIrrigation")
            .WithSummary("Get a paginated list of power/irrigation codes")
            .WithDescription("Retrieves power/irrigation codes with filtering, sorting, and pagination")
            .Produces(StatusCodes.Status200OK);
        }
    }
}
