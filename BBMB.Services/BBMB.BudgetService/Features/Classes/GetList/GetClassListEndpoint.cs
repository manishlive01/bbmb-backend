using BBMB.BudgetService.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Classes.GetList
{
    public class GetClassListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/classes", async (
                IMediator mediator,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? sortBy = null,
                [FromQuery] bool sortDescending = false,
                [FromQuery] string? classCode = null,
                [FromQuery] string? classDesc = null,
                [FromQuery] DateTime? modificationDateFrom = null,
                [FromQuery] DateTime? modificationDateTo = null) =>
            {
                var query = new GetClassListQuery(
                    pageNumber, pageSize, sortBy, sortDescending,
                    classCode, classDesc,
                    modificationDateFrom, modificationDateTo);

                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetClassList")
            .WithTags("Classes")
            .WithSummary("Get a paginated list of classes")
            .WithDescription("Retrieves classes with filtering, sorting, and pagination")
            .Produces(StatusCodes.Status200OK);
        }
    }
}
