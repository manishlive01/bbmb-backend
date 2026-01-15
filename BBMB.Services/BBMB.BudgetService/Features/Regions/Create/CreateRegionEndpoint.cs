using BBMB.BudgetService.Features.Regions.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Regions.Create
{
    public class CreateRegionEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/Create-Region", async (
                CreateRegionDto dto,
                IMediator mediator) =>
            {
                var command = new CreateRegionCommand(dto);

                var response = await mediator.Send(command);
                if (response.IsSucced == false)
                {
                    return Results.BadRequest(response);
                }

                return Results.Ok(response);
            })
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Create Region")
            .WithDescription("Creates a new Region entry")
            .WithName("Create Region")
            .WithTags("Region");
        }
    }
}
