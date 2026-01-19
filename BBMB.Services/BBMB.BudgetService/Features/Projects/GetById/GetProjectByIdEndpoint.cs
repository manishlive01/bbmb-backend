using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Projects.GetById
{
    public class GetProjectByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/projects/{code}", async (string code, IMediator mediator) =>
            {
                try
                {
                    var query = new GetProjectByIdQuery(code);
                    var result = await mediator.Send(query);
                    return Results.Ok(new
                    {
                        message = "Project retrieved successfully",
                        data = result
                    });
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new
                    {
                        error = "project_not_found",
                        message = ex.Message,
                        timestamp = DateTime.UtcNow
                    });
                }
            })
            .WithName("GetProjectById")
            .WithTags("Projects")
            .WithSummary("Get a project by code")
            .WithDescription("Retrieves a specific project by its code")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
