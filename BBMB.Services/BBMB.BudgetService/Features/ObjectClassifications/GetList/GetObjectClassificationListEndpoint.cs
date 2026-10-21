using BBMB.BudgetService.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.ObjectClassifications.GetList
{
    public class GetObjectClassificationListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/object-classifications", async (
                IMediator mediator,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? sortBy = null,
                [FromQuery] bool sortDescending = false,
                [FromQuery] string? objectAccCode = null,
                [FromQuery] string? objectAccDesc = null,
                [FromQuery] string? userCode = null,
                [FromQuery] DateTime? modificationDateFrom = null,
                [FromQuery] DateTime? modificationDateTo = null) =>
            {
                var query = new GetObjectClassificationListQuery(
                    pageNumber, pageSize, sortBy, sortDescending,
                    objectAccCode, objectAccDesc, userCode,
                    modificationDateFrom, modificationDateTo);

                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .RequireAuthorization("BudgetRight:Budget.View")
            .WithName("GetObjectClassificationList")
            .WithTags("ObjectClassifications")
            .WithSummary("Get a paginated list of object classifications")
            .WithDescription("Retrieves object classifications with filtering, sorting, and pagination")
            .Produces(StatusCodes.Status200OK);
        }
    }
}
