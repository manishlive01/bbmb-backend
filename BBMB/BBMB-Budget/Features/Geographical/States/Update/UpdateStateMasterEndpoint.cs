using BBMB_Budget.Features.Geographical.States.Update.Dtos;
using BBMB_Budget.Shared.Endpoints;
using MediatR;

namespace BBMB_Budget.Features.Geographical.States.Update
{
    public class UpdateStateMasterEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/Update-State", async (
                UpdateStateMasterDto dto,
                IMediator mediator) =>
            {
                var command = new UpdateStateMasterCommand(dto);

                var response = await mediator.Send(command);
                if (response.IsSucced == false)
                {
                    return Results.BadRequest(response);
                }
                return Results.Ok(response);
            })
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Update State Master")
            .WithDescription("Updates a new State Master entry")
            .WithName("Update State Master")
            .WithTags("StateMaster");
        }
    }
}
