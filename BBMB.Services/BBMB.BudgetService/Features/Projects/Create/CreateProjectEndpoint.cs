using BBMB.BudgetService.Features.Projects.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Projects.Create
{
    public class CreateProjectEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/projects", async (CreateProjectDto dto, IMediator mediator) =>
            {
                var command = new CreateProjectCommand(dto);
                var response = await mediator.Send(command);

                if (!response.IsSucced)
                {
                    return Results.Conflict(response);
                }

                return Results.Created($"/api/projects/{dto.ProjCode}", response);
            })
            .WithName("CreateProject")
            .WithTags("Projects")
            .WithSummary("Create a new project")
            .WithDescription("Creates a new project entry")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict);
        }
    }
}
