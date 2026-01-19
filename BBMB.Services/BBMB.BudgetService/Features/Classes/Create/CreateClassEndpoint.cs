using BBMB.BudgetService.Features.Classes.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Classes.Create
{
    public class CreateClassEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/classes", async (CreateClassDto dto, IMediator mediator) =>
            {
                var command = new CreateClassCommand(dto);
                var response = await mediator.Send(command);

                if (!response.IsSucced)
                {
                    return Results.Conflict(response);
                }

                return Results.Created($"/api/classes/{dto.ClassCode}", response);
            })
            .WithName("CreateClass")
            .WithTags("Classes")
            .WithSummary("Create a new class")
            .WithDescription("Creates a new class entry")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict);
        }
    }
}
