using BBMB.BudgetService.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Projects.GetList
{
    public class GetProjectListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/projects", async (
                IMediator mediator,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? sortBy = null,
                [FromQuery] bool sortDescending = false,
                [FromQuery] string? projCode = null,
                [FromQuery] string? projDesc = null,
                [FromQuery] string? userCode = null,
                [FromQuery] DateTime? modificationDateFrom = null,
                [FromQuery] DateTime? modificationDateTo = null) =>
            {
                var query = new GetProjectListQuery(
                    pageNumber, pageSize, sortBy, sortDescending,
                    projCode, projDesc, userCode,
                    modificationDateFrom, modificationDateTo);

                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetProjectList")
            .WithTags("Projects")
            .WithSummary("Get a paginated list of projects")
            .WithDescription("Retrieves projects with filtering, sorting, and pagination")
            .Produces(StatusCodes.Status200OK);
        }
    }
}
