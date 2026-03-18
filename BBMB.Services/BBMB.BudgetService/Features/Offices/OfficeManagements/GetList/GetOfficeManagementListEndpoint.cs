using BBMB.BudgetService.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Offices.OfficeManagements.GetList
{
    public class GetOfficeManagementListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/office-managements", async (
                IMediator mediator,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? sortBy = null,
                [FromQuery] bool sortDescending = false,
                [FromQuery] string? officeCode = null,
                [FromQuery] string? officeDesc = null,
                [FromQuery] int? ceCode = null,
                [FromQuery] int? crCode = null,
                [FromQuery] int? divisionCode = null,
                [FromQuery] string? userCode = null,
                [FromQuery] DateTime? modificationDateFrom = null,
                [FromQuery] DateTime? modificationDateTo = null
            ) =>
            {
                var query = new GetOfficeManagementListQuery(
                    pageNumber, pageSize, sortBy, sortDescending,
                    officeCode, officeDesc, ceCode, crCode, divisionCode,
                    userCode, modificationDateFrom, modificationDateTo);

                var result = await mediator.Send(query);

                return Results.Ok(result);
            })
            .WithName("GetOfficeManagementList")
            .WithTags("Office Management")
            .WithSummary("Get office management list")
            .WithDescription("Retrieve office management data with filtering, sorting, and pagination")
            .Produces(StatusCodes.Status200OK);
        }
    }
}