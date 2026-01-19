using BBMB.BudgetService.Features.Regions.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Regions.Update
{
    public class UpdateRegionEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/regions/{code}", async (string code, UpdateRegionDto dto, IMediator mediator) =>
            {
                var command = new UpdateRegionCommand(code, dto);
                var response = await mediator.Send(command);

                return response.IsSucced
                    ? Results.Ok(response)
                    : Results.NotFound(response);
            })
            .WithName("UpdateRegion")
            .WithTags("Regions")
            .WithSummary("Update a region")
            .WithDescription("Updates an existing region's description and user code")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
