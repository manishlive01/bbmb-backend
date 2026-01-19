using BBMB.BudgetService.Features.Pools.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Pools.Create
{
    public class CreatePoolEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/pools", async (CreatePoolDto dto, IMediator mediator) =>
            {
                var command = new CreatePoolCommand(dto);
                var response = await mediator.Send(command);

                if (!response.IsSucced)
                {
                    return Results.Conflict(response);
                }

                return Results.Created($"/api/pools/{dto.PoolCode}", response);
            })
            .WithName("CreatePool")
            .WithTags("Pools")
            .WithSummary("Create a new pool")
            .WithDescription("Creates a new pool entry")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict);
        }
    }
}
