using BBMB.BudgetService.Features.HierarchyManagement.Divisions.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.Divisions.Update
{
    public class UpdateDivisionEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/divisions/{code:int}", async (int divId, UpdateDivisionDto dto, IMediator mediator) =>
            {
                var command = new UpdateDivisionCommand(divId, dto);
                var response = await mediator.Send(command);

                return response.IsSucced
                    ? Results.Ok(response)
                    : Results.NotFound(response);
            })
            .WithName("UpdateDivision")
            .WithTags("Divisions")
            .WithSummary("Update a division")
            .WithDescription("Updates an existing division's details")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
