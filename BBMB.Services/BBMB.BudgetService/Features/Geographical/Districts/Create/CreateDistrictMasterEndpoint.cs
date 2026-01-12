using BBMB.BudgetService.Features.Geographical.Districts.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Geographical.Districts.Create
{
    public class CreateDistrictMasterEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/Create-District", async (
                CreateDistrictMasterDto dto,
                IMediator mediator) =>
            {
                var command = new CreateDistrictMasterCommand(dto);

                var response = await mediator.Send(command);
                if (response.IsSucced == false)
                {
                    return Results.BadRequest(response);
                }
                return Results.Ok(response);
            })
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Create District Master")
            .WithDescription("Creates a new District Master entry")
            .WithName("Create District Master")
            .WithTags("DistrictMaster");
        }
    }
}
