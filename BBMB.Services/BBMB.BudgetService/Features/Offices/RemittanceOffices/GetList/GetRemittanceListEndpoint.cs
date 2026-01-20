using BBMB.BudgetService.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Offices.RemittanceOffices.GetList
{
    public class GetRemittanceListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/remittance", async (
                IMediator mediator,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? sortBy = null,
                [FromQuery] bool sortDescending = false,
                [FromQuery] string? poolCode = null,
                [FromQuery] string? poolDesc = null,
                [FromQuery] string? userCode = null,
                [FromQuery] DateTime? modificationDateFrom = null,
                [FromQuery] DateTime? modificationDateTo = null) =>
            {
                var query = new GetRemittanceListQuery(
                    pageNumber, pageSize, sortBy, sortDescending,
                    poolCode, poolDesc, userCode,
                    modificationDateFrom, modificationDateTo);

                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetRemitanceList")
          .WithTags("OfficeRemitance")
            .WithSummary("Get a paginated list of Remitance")
            .WithDescription("Retrieves Remitance with filtering, sorting, and pagination")
            .Produces(StatusCodes.Status200OK);
        }
    }
}
