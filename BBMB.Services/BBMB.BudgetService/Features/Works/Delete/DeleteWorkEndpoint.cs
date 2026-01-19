using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Works.Delete
{
    public class DeleteWorkEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/works/{code}", async (string code, IMediator mediator) =>
            {
                var command = new DeleteWorkCommand(code);
                var response = await mediator.Send(command);

                if (!response.IsSucced)
                {
                    if (response.Message.Contains("not found"))
                        return Results.NotFound(response);
                    else
                        return Results.Conflict(response);
                }

                return Results.NoContent();
            })
            .WithName("DeleteWork")
            .WithTags("Works")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<Response>(StatusCodes.Status404NotFound)
            .Produces<Response>(StatusCodes.Status409Conflict);
        }
    }
}
