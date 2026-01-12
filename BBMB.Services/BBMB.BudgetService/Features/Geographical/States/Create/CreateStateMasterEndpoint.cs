using BBMB.BudgetService.Features.Geographical.States.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Geographical.States.Create
{
    public class CreateStateMasterEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/Create-State", async (
                CreateStateMasterDto dto,
                IMediator mediator) =>
            {
                var command = new CreateStateMasterCommand(dto);

                var response = await mediator.Send(command);
                if (response.IsSucced == false)
                {
                    return Results.BadRequest(response);
                }
                return Results.Ok(response);
            })
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Create State Master")
            .WithDescription("Creates a new State Master entry")
            .WithName("Create State Master")
            .WithTags("StateMaster");
        }
    }
}
