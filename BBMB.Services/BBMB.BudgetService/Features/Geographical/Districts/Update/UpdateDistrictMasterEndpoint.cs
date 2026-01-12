using BBMB.BudgetService.Features.Geographical.Districts.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Geographical.Districts.Update
{
    public class UpdateDistrictMasterEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/Update-District", async (
                UpdateDistrictMasterDto dto,
                IMediator mediator) =>
            {
                var command = new UpdateDistrictMasterCommand(dto);

                var response = await mediator.Send(command);
                if (response.IsSucced == false)
                {
                    return Results.BadRequest(response);
                }
                return Results.Ok(response);
            })
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Update District Master")
            .WithDescription("Updates a new District Master entry")
            .WithName("Update District Master")
            .WithTags("DistrictMaster");
        }
    }
}
