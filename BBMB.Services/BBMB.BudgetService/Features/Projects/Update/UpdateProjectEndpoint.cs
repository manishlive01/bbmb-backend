using BBMB.BudgetService.Features.Projects.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Projects.Update
{
    public class UpdateProjectEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/projects/{code}", async (string code, UpdateProjectDto dto, IMediator mediator) =>
            {
                var command = new UpdateProjectCommand(code, dto);
                var response = await mediator.Send(command);

                return response.IsSucced
                    ? Results.Ok(response)
                    : Results.NotFound(response);
            })
            .WithName("UpdateProject")
            .WithTags("Projects")
            .WithSummary("Update a project")
            .WithDescription("Updates an existing project's description and user code")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
