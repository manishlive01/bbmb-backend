using BBMB.BudgetService.Features.Designations.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Designations.Update
{
    public class UpdateDesignationEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/designations/{code}", async (string code, UpdateDesignationDto dto, IMediator mediator) =>
            {
                var command = new UpdateDesignationCommand(code, dto);
                var response = await mediator.Send(command);

                return response.IsSucced
                    ? Results.Ok(response)
                    : Results.NotFound(response);
            })
            .WithName("UpdateDesignation")
            .WithTags("Designations")
            .WithSummary("Update a designation")
            .WithDescription("Updates an existing designation's description and user code")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
