using BBMB.BudgetService.Features.Pools.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Pools.Update
{
    public class UpdatePoolEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/pools/{code}", async (string code, UpdatePoolDto dto, IMediator mediator) =>
            {
                var command = new UpdatePoolCommand(code, dto);
                var response = await mediator.Send(command);

                return response.IsSucced
                    ? Results.Ok(response)
                    : Results.NotFound(response);
            })
            .WithName("UpdatePool")
            .WithTags("Pools")
            .WithSummary("Update a pool")
            .WithDescription("Updates an existing pool's description and user code")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
