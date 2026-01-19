using BBMB.BudgetService.Features.Units.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Units.Update
{
    public class UpdateUnitEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/units/{code}", async (string code, UpdateUnitDto dto, IMediator mediator) =>
            {
                var command = new UpdateUnitCommand(code, dto);
                var response = await mediator.Send(command);

                return response.IsSucced
                    ? Results.Ok(response)
                    : Results.NotFound(response);
            })
            .WithName("UpdateUnit")
            .WithTags("Units")
            .WithSummary("Update a unit")
            .WithDescription("Updates an existing unit's description, project code, power/irrigation code, and user code")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
