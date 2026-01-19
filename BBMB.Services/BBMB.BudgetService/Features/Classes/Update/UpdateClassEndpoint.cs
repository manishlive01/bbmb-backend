using BBMB.BudgetService.Features.Classes.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Classes.Update
{
    public class UpdateClassEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/classes/{code}", async (string code, UpdateClassDto dto, IMediator mediator) =>
            {
                var command = new UpdateClassCommand(code, dto);
                var response = await mediator.Send(command);

                return response.IsSucced
                    ? Results.Ok(response)
                    : Results.NotFound(response);
            })
            .WithName("UpdateClass")
            .WithTags("Classes")
            .WithSummary("Update a class")
            .WithDescription("Updates an existing class's description")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
