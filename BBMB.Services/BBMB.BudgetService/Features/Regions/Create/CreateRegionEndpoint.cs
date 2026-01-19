using BBMB.BudgetService.Features.Regions.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Regions.Create
{
    public class CreateRegionEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/regions", async (CreateRegionDto dto, IMediator mediator) =>
            {
                var command = new CreateRegionCommand(dto);
                var response = await mediator.Send(command);

                if (!response.IsSucced)
                {
                    return Results.Conflict(response);
                }

                return Results.Created($"/api/regions/{dto.RegionCode}", response);
            })
            .WithName("CreateRegion")
            .WithTags("Regions")
            .WithSummary("Create a new region")
            .WithDescription("Creates a new region entry")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict);
        }
    }
}
