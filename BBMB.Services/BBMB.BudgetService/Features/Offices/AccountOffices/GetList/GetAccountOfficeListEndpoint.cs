using BBMB.BudgetService.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Offices.AccountOffices.GetList
{
    public class GetAccountOfficeListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/account-offices", async (
                IMediator mediator,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? sortBy = null,
                [FromQuery] bool sortDescending = false,
                [FromQuery] string? accOffice = null,
                [FromQuery] string? accOfficeDesc = null,
                [FromQuery] string? userCode = null,
                [FromQuery] DateTime? modificationDateFrom = null,
                [FromQuery] DateTime? modificationDateTo = null) =>
            {
                var query = new GetAccountOfficeListQuery(
                    pageNumber,
                    pageSize,
                    sortBy,
                    sortDescending,
                    accOffice,
                    accOfficeDesc,
                    userCode,
                    modificationDateFrom,
                    modificationDateTo);

                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetAccountOfficeList")
            .WithTags("AccountOffices")
            .WithSummary("Get a paginated list of account offices")
            .WithDescription("Retrieves account offices with filtering, sorting, and pagination")
            .Produces(StatusCodes.Status200OK);
        }
    }
}
